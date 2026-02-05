import pika
import json
import subprocess
import os
import signal
import sys
import logging
import time

from src.core.config import config

# Configure logging
logging.basicConfig(level=config.LOG_LEVEL, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


def establish_rabbitmq_connection():
    """Establishes and returns a connection and channel to RabbitMQ."""
    try:
        credentials = pika.PlainCredentials(config.RABBITMQ_USER, config.RABBITMQ_PASS)
        connection_params = pika.ConnectionParameters(
            host=config.RABBITMQ_HOST,
            port=config.RABBITMQ_PORT,
            credentials=credentials,
            heartbeat=config.RABBITMQ_HEARTBEAT,
            blocked_connection_timeout=config.RABBITMQ_BLOCKED_CONNECTION_TIMEOUT
        )
        connection = pika.BlockingConnection(connection_params)
        channel = connection.channel()

        # Declare the queue for render requests
        channel.queue_declare(queue=config.RENDER_REQUEST_QUEUE, durable=True)
        # Declare the exchange for render requests and bind the queue to it
        channel.exchange_declare(exchange=config.RENDER_REQUEST_EXCHANGE, exchange_type='topic', durable=True)
        channel.queue_bind(
            queue=config.RENDER_REQUEST_QUEUE,
            exchange=config.RENDER_REQUEST_EXCHANGE,
            routing_key=config.RENDER_REQUEST_QUEUE
        )

        # Declare the exchange for status updates
        channel.exchange_declare(exchange=config.STATUS_UPDATE_EXCHANGE, exchange_type='topic', durable=True)
        
        logger.info(f"Successfully connected to RabbitMQ at {config.RABBITMQ_HOST}:{config.RABBITMQ_PORT}")
        return connection, channel
    except pika.exceptions.AMQPConnectionError as e:
        logger.error(f"Failed to connect to RabbitMQ: {e}. The service will attempt to reconnect.")
        return None, None
    except Exception as e:
        logger.error(f"An unexpected error occurred during RabbitMQ connection: {e}. The service will attempt to reconnect.")
        return None, None


from src.core.domain import RenderResponse # Add import

def publish_response(channel, job_id, status, output_file_path=None, error_message=None):
    """Publishes a response message to the backend for status updates."""
    payload = RenderResponse(
        job_id=job_id,
        status=status,
        output_file_path=output_file_path,
        error_message=error_message
    )

    try:
        if not channel or not channel.is_open:
            logger.warning("RabbitMQ channel is closed, response cannot be published for job %s.", job_id)
            return

        channel.basic_publish(
            exchange=config.STATUS_UPDATE_EXCHANGE,
            routing_key=config.STATUS_UPDATE_ROUTING_KEY,
            body=payload.model_dump_json(), # Use model_dump_json for Pydantic v2
            properties=pika.BasicProperties(
                delivery_mode=2,  # make message persistent
            )
        )
        logger.info("Published response for job %s to exchange '%s' with routing key '%s': %s",
                    job_id, config.STATUS_UPDATE_EXCHANGE, config.STATUS_UPDATE_ROUTING_KEY, status)
    except Exception as e:
        logger.error("Failed to publish response for job %s: %s", job_id, e)

def ensure_output_directory():
    """Ensures the output directory exists."""
    os.makedirs(config.OUTPUT_DIR, exist_ok=True)
    logger.info("Ensured output directory exists: %s", config.OUTPUT_DIR)

from src.core.domain import RenderRequest
from src.core.services import GraphvizService

def process_render_request(ch, method, properties, body):
    """Callback function to process incoming render requests."""
    job_id = "unknown" # Default job_id for logging in case of parsing error
    try:
        request_data = json.loads(body)
        request = RenderRequest(**request_data)
        job_id = request.job_id # Update job_id for logging

        logger.info("Received render request for job_id: %s, dot_filename: %s", request.job_id, request.dot_filename)

        ensure_output_directory()

        try:
            output_file_path = GraphvizService.render_dot_to_pdf(request)
            publish_response(ch, request.job_id, "success", output_file_path=output_file_path)
        except (FileNotFoundError, subprocess.TimeoutExpired, RuntimeError) as e:
            error_msg = f"Graphviz rendering failed for job {request.job_id}: {e}"
            logger.error(error_msg)
            publish_response(ch, request.job_id, "error", error_message=error_msg)

    except json.JSONDecodeError:
        error_msg = f"Invalid JSON received: {body.decode()}"
        logger.error(error_msg)
        # Cannot publish response as job_id is unknown
    except ValueError as e: # Pydantic ValidationError inherits from ValueError
        error_msg = f"Request validation failed for job {job_id}: {e}"
        logger.error(error_msg)
        publish_response(ch, job_id, "error", error_message=error_msg)
    except Exception as e:
        error_msg = f"An unhandled error occurred during processing for job {job_id}: {e}"
        logger.error(error_msg, exc_info=True)
        publish_response(ch, job_id, "error", error_message=error_msg)
    finally:
        ch.basic_ack(method.delivery_tag)
        logger.info("Acknowledged message for job %s", job_id)

def graceful_shutdown(signum, frame, connection, channel):
    """Handles graceful shutdown on SIGTERM."""
    logger.info("SIGTERM received, initiating graceful shutdown...")
    if channel and channel.is_open:
        channel.stop_consuming()
        logger.info("Stopped consuming messages.")
    if connection and connection.is_open:
        connection.close()
        logger.info("Closed RabbitMQ connection.")
    logger.info("Shutdown complete.")
    sys.exit(0)

def main():
    """Main function to start the RabbitMQ consumer."""
    logger.info("Starting Graphviz PDF Renderer service...")

    # Ensure output directory exists at startup
    ensure_output_directory()

    connection = None
    channel = None
    reconnect_delay = 5 # seconds

    while True: # Loop to continuously try connecting
        try:
            logger.info("Attempting to connect to RabbitMQ...")
            connection, channel = establish_rabbitmq_connection()

            if connection is None or channel is None:
                logger.error(f"Failed to establish RabbitMQ connection. Retrying in {reconnect_delay} seconds...")
                time.sleep(reconnect_delay)
                continue # Try again

            # Register the graceful shutdown handler with connection and channel
            # This needs to be done *after* a successful connection is established
            signal.signal(signal.SIGTERM, lambda signum, frame: graceful_shutdown(signum, frame, connection, channel))

            logger.info("Waiting for messages in %s. To exit press CTRL+C", config.RENDER_REQUEST_QUEUE)

            channel.basic_qos(prefetch_count=1) # Process one message at a time
            channel.basic_consume(queue=config.RENDER_REQUEST_QUEUE, on_message_callback=lambda ch, method, properties, body: process_render_request(ch, method, properties, body))
            channel.start_consuming()

        except KeyboardInterrupt:
            logger.info("KeyboardInterrupt received, exiting.")
            graceful_shutdown(None, None, connection, channel) # Pass existing connection/channel
            break # Exit the while loop
        except pika.exceptions.ConnectionClosedByBroker:
            logger.error("RabbitMQ connection closed by broker. Attempting to reconnect in %s seconds...", reconnect_delay)
            time.sleep(reconnect_delay)
            # Loop will naturally try to re-establish connection
        except pika.exceptions.AMQPChannelError as e:
            logger.error(f"RabbitMQ channel error: {e}. Attempting to reconnect in {reconnect_delay} seconds...")
            if connection and connection.is_open:
                connection.close() # Close the connection to ensure a fresh start
            time.sleep(reconnect_delay)
        except Exception as e:
            logger.error("An unexpected error occurred during message consumption: %s. Attempting to reconnect in %s seconds...", e, reconnect_delay, exc_info=True)
            if connection and connection.is_open:
                connection.close()
            time.sleep(reconnect_delay)

        finally:
            # Ensure connection is closed if not already (e.g., after an exception in start_consuming)
            if connection and connection.is_open:
                connection.close()
                logger.info("Closed RabbitMQ connection in finally block.")
