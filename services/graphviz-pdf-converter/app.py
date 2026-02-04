import pika
import json
import subprocess
import os
import signal
import sys
import logging
import time

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# --- Configuration Constants ---
RABBITMQ_HOST = os.environ.get('RABBITMQ__HOSTNAME', 'rabbitmq')
RABBITMQ_PORT = int(os.environ.get('RABBITMQ__PORT', 5672))
RABBITMQ_USER = os.environ.get('RABBITMQ__USERNAME', 'guest')
RABBITMQ_PASS = os.environ.get('RABBITMQ__PASSWORD', 'guest')

INPUT_DIR = '/shared/input'
OUTPUT_DIR = '/shared/output'

# Queues for inbound and outbound messages
RENDER_REQUEST_QUEUE = 'render_request' # Backend publishes to this
RENDER_REQUEST_EXCHANGE = 'render_request' # Backend publishes to this exchange
STATUS_UPDATE_EXCHANGE = 'graph_generation_exchange' # Python publishes to this
STATUS_UPDATE_ROUTING_KEY = 'graph.status.updated' # Python publishes with this routing key

RENDER_TIMEOUT_SECONDS = 120 # Timeout for Graphviz dot command

connection = None
channel = None

def establish_rabbitmq_connection():
    """Establishes and returns a connection and channel to RabbitMQ."""
    global connection, channel
    try:
        credentials = pika.PlainCredentials(RABBITMQ_USER, RABBITMQ_PASS)
        connection_params = pika.ConnectionParameters(
            host=RABBITMQ_HOST,
            port=RABBITMQ_PORT,
            credentials=credentials,
            heartbeat=600 # Set a heartbeat to prevent connection issues
        )
        connection = pika.BlockingConnection(connection_params)
        channel = connection.channel()

        # Declare the queue for render requests
        channel.queue_declare(queue=RENDER_REQUEST_QUEUE, durable=True)
        # Declare the exchange for render requests and bind the queue to it
        channel.exchange_declare(exchange=RENDER_REQUEST_EXCHANGE, exchange_type='topic', durable=True)
        channel.queue_bind(
            queue=RENDER_REQUEST_QUEUE,
            exchange=RENDER_REQUEST_EXCHANGE,
            routing_key=RENDER_REQUEST_QUEUE # Routing key is also 'render_request' based on backend logs
        )

        # Declare the exchange for status updates
        channel.exchange_declare(exchange=STATUS_UPDATE_EXCHANGE, exchange_type='topic', durable=True)
        
        logger.info(f"Successfully connected to RabbitMQ at {RABBITMQ_HOST}:{RABBITMQ_PORT}")
        return channel
    except pika.exceptions.AMQPConnectionError as e:
        logger.error(f"Failed to connect to RabbitMQ: {e}")
        # Exit if connection fails at startup
        sys.exit(1)
    except Exception as e:
        logger.error(f"An unexpected error occurred during RabbitMQ connection: {e}")
        sys.exit(1)


def publish_response(job_id, status, output_file_path=None, error_message=None):
    """Publishes a response message to the backend for status updates."""
    payload = {
        "job_id": job_id,
        "status": status,
    }
    if output_file_path:
        payload["output_file_path"] = output_file_path # Renamed from pdf_path
    if error_message:
        payload["error_message"] = error_message

    try:
        if not channel or not channel.is_open:
            logger.warning("RabbitMQ channel is closed, attempting to re-establish connection...")
            establish_rabbitmq_connection() # Re-establish connection if it was closed

        channel.basic_publish(
            exchange=STATUS_UPDATE_EXCHANGE, # Publish to the exchange
            routing_key=STATUS_UPDATE_ROUTING_KEY, # Use the routing key
            body=json.dumps(payload),
            properties=pika.BasicProperties(
                delivery_mode=2,  # make message persistent
            )
        )
        logger.info(f"Published response for job {job_id} to exchange '{STATUS_UPDATE_EXCHANGE}' with routing key '{STATUS_UPDATE_ROUTING_KEY}': {status}")
    except Exception as e:
        logger.error(f"Failed to publish response for job {job_id}: {e}")

def ensure_output_directory():
    """Ensures the output directory exists."""
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    logger.info(f"Ensured output directory exists: {OUTPUT_DIR}")

def process_render_request(ch, method, properties, body):
    """Callback function to process incoming render requests."""
    job_id = "unknown" # Default job_id for logging in case of parsing error
    try:
        request = json.loads(body)
        job_id = request.get("job_id")
        dot_filename = request.get("dot_filename")
        page_size = request.get("page_size", "A0") # Default to A0 if not specified
        direction = request.get("direction", "LR") # Default to Left-to-Right

        if not all([job_id, dot_filename]):
            raise ValueError("Missing 'job_id' or 'dot_filename' in request.")

        logger.info(f"Received render request for job_id: {job_id}, dot_filename: {dot_filename}")

        input_dot_path = os.path.join(INPUT_DIR, dot_filename)
        output_pdf_path = os.path.join(OUTPUT_DIR, f"{job_id}.pdf")

        # 1. Validate input .dot file existence
        if not os.path.exists(input_dot_path):
            error_msg = f"Input .dot file not found: {input_dot_path}"
            logger.error(error_msg)
            publish_response(job_id, "error", error_message=error_msg)
            # ch.basic_ack(method.delivery_tag) # Removed: Acknowledged in finally block
            return

        # 2. Ensure output directory exists
        ensure_output_directory()

        # 3. Run Graphviz dot command
        # Example command: dot -Kdot -Tpdf -Gdpi=300 -Grankdir=LR -sA0 input.dot -o output.pdf
        # -Kdot: specifies layout engine (default for .dot files)
        # -Tpdf: output format
        # -Gdpi=300: sets DPI for better quality
        # -Grankdir: graph direction
        # -s: sets page size (Graphviz -s option expects page size, not -P)
        command = [
            'dot',
            '-Tpdf',
            f'-Grankdir={direction}',
            f'-s{page_size}', # Use -s for page size
            input_dot_path,
            '-o',
            output_pdf_path
        ]
        logger.info(f"Executing command: {' '.join(command)}")

        try:
            result = subprocess.run(
                command,
                capture_output=True,
                text=True,
                timeout=RENDER_TIMEOUT_SECONDS,
                check=False # Do not raise CalledProcessError automatically
            )

            if result.returncode == 0:
                logger.info(f"Successfully rendered PDF for job {job_id} at {output_pdf_path}")
                publish_response(job_id, "success", output_file_path=output_pdf_path)
            else:
                error_msg = (
                    f"Graphviz rendering failed for job {job_id}. "
                    f"Return code: {result.returncode}. "
                    f"Stdout: {result.stdout.strip()}. "
                    f"Stderr: {result.stderr.strip()}."
                )
                logger.error(error_msg)
                publish_response(job_id, "error", error_message=error_msg)

        except subprocess.TimeoutExpired:
            error_msg = f"Graphviz command timed out after {RENDER_TIMEOUT_SECONDS} seconds for job {job_id}."
            logger.error(error_msg)
            publish_response(job_id, "error", error_message=error_msg)
        except FileNotFoundError:
            error_msg = "Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"
            logger.error(error_msg)
            publish_response(job_id, "error", error_message=error_msg)
        except Exception as e:
            error_msg = f"An unexpected error occurred during subprocess execution for job {job_id}: {e}"
            logger.error(error_msg)
            publish_response(job_id, "error", error_message=error_msg)

    except json.JSONDecodeError:
        error_msg = f"Invalid JSON received: {body.decode()}"
        logger.error(error_msg)
        # Cannot publish response as job_id is unknown
    except ValueError as e:
        error_msg = f"Request validation failed for job {job_id}: {e}"
        logger.error(error_msg)
        publish_response(job_id, "error", error_message=error_msg)
    except Exception as e:
        error_msg = f"An unhandled error occurred during processing for job {job_id}: {e}"
        logger.error(error_msg, exc_info=True) # Log traceback for unhandled exceptions
        publish_response(job_id, "error", error_message=error_msg)
    finally:
        # Acknowledge the message only after attempting to process it
        # This ensures messages are not lost if processing fails but we still want to
        # send an error response and move on.
        ch.basic_ack(method.delivery_tag)
        logger.info(f"Acknowledged message for job {job_id}")

def graceful_shutdown(signum, frame):
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
    signal.signal(signal.SIGTERM, graceful_shutdown)
    logger.info("Starting Graphviz PDF Renderer service...")

    # Ensure output directory exists at startup
    ensure_output_directory()

    # Establish RabbitMQ connection
    global channel
    channel = establish_rabbitmq_connection()

    logger.info(f"Waiting for messages in {RENDER_REQUEST_QUEUE}. To exit press CTRL+C")

    # Start consuming messages
    try:
        channel.basic_qos(prefetch_count=1) # Process one message at a time
        channel.basic_consume(queue=RENDER_REQUEST_QUEUE, on_message_callback=process_render_request)
        channel.start_consuming()
    except KeyboardInterrupt:
        logger.info("KeyboardInterrupt received, exiting.")
        graceful_shutdown(None, None)
    except pika.exceptions.ConnectionClosedByBroker:
        logger.error("RabbitMQ connection closed by broker, attempting to reconnect...")
        # This simple retry mechanism might be sufficient for transient issues
        # For production, consider a more robust retry logic with backoff
        time.sleep(5)
        main() # Attempt to restart
    except Exception as e:
        logger.error(f"An unexpected error occurred during message consumption: {e}")
        graceful_shutdown(None, None)

if __name__ == "__main__":
    main()
