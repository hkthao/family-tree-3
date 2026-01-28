import json
import logging
import os
from typing import Optional

import aio_pika
from aio_pika.abc import AbstractRobustConnection

from app.services.face_service import FaceService
from app.models import (
    MemberFaceAddedMessage,
    MemberFaceDeletedMessage,
    MessageBusConstants,
)

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

RABBITMQ_USERNAME = os.getenv("RABBITMQ__USERNAME", "guest")
RABBITMQ_PASSWORD = os.getenv("RABBITMQ__PASSWORD", "guest")
RABBITMQ_HOSTNAME = os.getenv("RABBITMQ__HOSTNAME", "rabbitmq")
RABBITMQ_PORT = os.getenv("RABBITMQ__PORT", "5672")

RABBITMQ_URL = f"amqp://{RABBITMQ_USERNAME}:{RABBITMQ_PASSWORD}@{RABBITMQ_HOSTNAME}:{RABBITMQ_PORT}/"


class MessageConsumer:
    """
    Consumes messages from RabbitMQ related to member face events.
    """

    def __init__(self, face_service: FaceService):
        self.face_service = face_service
        self.connection: Optional[AbstractRobustConnection] = None
        self.channel: Optional[aio_pika.abc.AbstractRobustChannel] = None
        self.queue: Optional[aio_pika.abc.AbstractRobustQueue] = None

    async def _connect(self):
        """Establishes connection to RabbitMQ."""
        logger.info(f"Connecting to RabbitMQ at {RABBITMQ_URL}")
        self.connection = await aio_pika.connect_robust(RABBITMQ_URL)
        self.channel = await self.connection.channel()
        logger.info("Successfully connected to RabbitMQ and opened channel.")

    async def _declare_exchange(self):
        """Declares the necessary exchange."""
        if not self.channel:
            raise RuntimeError("Channel not established. Call _connect first.")
        self.exchange = await self.channel.declare_exchange(
            MessageBusConstants.Exchanges.MEMBER_FACE, aio_pika.ExchangeType.TOPIC, durable=True
        )
        logger.info(f"Declared exchange: {MessageBusConstants.Exchanges.MEMBER_FACE}")

    async def _setup_queue(self):
        """Sets up the queue and binds it to routing keys."""
        if not self.channel:
            raise RuntimeError("Channel not established. Call _connect first.")

        self.queue = await self.channel.declare_queue(
            f"face_service_queue_{os.getenv('HOSTNAME', 'default')}",  # Unique queue per service instance
            durable=True,
            arguments={"x-expires": 1800000}  # Queue expires after 30 minutes of inactivity
        )
        logger.info(f"Declared queue: {self.queue.name}")

        # Bind for added messages
        await self.queue.bind(self.exchange, MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED)
        logger.info(
            f"Bound queue {self.queue.name} to {MessageBusConstants.Exchanges.MEMBER_FACE} "
            f"with routing key: {MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED}"
        )

        # Bind for deleted messages
        await self.queue.bind(self.exchange, MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED)
        logger.info(
            f"Bound queue {self.queue.name} to {MessageBusConstants.Exchanges.MEMBER_FACE} "
            f"with routing key: {MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED}"
        )

    async def _on_message_added(self, message: aio_pika.abc.AbstractIncomingMessage):
        """Callback for MemberFaceAddedMessage."""
        async with message.process():
            try:
                logger.info(f"Received MemberFaceAddedMessage: {message.routing_key}")
                data = json.loads(message.body.decode())
                added_message = MemberFaceAddedMessage.model_validate(data)

                face_add_request = added_message.face_add_request
                vector = face_add_request.vector
                metadata = face_add_request.metadata.model_dump()

                # The BoundingBox model has float fields, but FaceService's BoundingBox expects int.
                # Need to convert BoundingBox fields to int.
                if "bounding_box" in metadata and metadata["bounding_box"] is not None:
                    bbox = metadata["bounding_box"]
                    metadata["bounding_box"] = {
                        "x": int(bbox["x"]),
                        "y": int(bbox["y"]),
                        "width": int(bbox["width"]),
                        "height": int(bbox["height"]),
                    }

                self.face_service.add_face_by_vector(vector, metadata)
                logger.info(f"Metadata passed to add_face_by_vector: {metadata}")
                logger.info(
                    f"Processed MemberFaceAddedMessage for FaceId: {metadata['face_id']} "
                    f"from MemberFaceLocalId: {added_message.member_face_local_id}"
                )
            except json.JSONDecodeError:
                logger.error(f"Failed to decode JSON from message: {message.body}", exc_info=True)
            except Exception as e:
                logger.error(f"Error processing MemberFaceAddedMessage: {e}", exc_info=True)
                # Requeue message if processing failed (e.g., database error)
                # or send to dead-letter queue
                # For now, we'll let it be acknowledged to avoid reprocessing indefinitely
                # In a real-world scenario, you might want to NACK and requeue
                # or move to a dead-letter queue after several retries.

    async def _on_message_deleted(self, message: aio_pika.abc.AbstractIncomingMessage):
        """Callback for MemberFaceDeletedMessage."""
        async with message.process():
            try:
                logger.info(f"Received MemberFaceDeletedMessage: {message.routing_key}")
                data = json.loads(message.body.decode())
                deleted_message = MemberFaceDeletedMessage.model_validate(data)

                vector_db_id = deleted_message.vector_db_id
                if vector_db_id:
                    await self.face_service.delete_face(vector_db_id)
                    logger.info(
                        f"Processed MemberFaceDeletedMessage for VectorDbId: {vector_db_id} "
                        f"from MemberFaceId: {deleted_message.member_face_id}"
                    )
                else:
                    logger.warning(
                        f"MemberFaceDeletedMessage for MemberFaceId: {deleted_message.member_face_id} "
                        "has no VectorDbId. Skipping deletion from vector DB."
                    )
            except json.JSONDecodeError:
                logger.error(f"Failed to decode JSON from message: {message.body}", exc_info=True)
            except Exception as e:
                logger.error(f"Error processing MemberFaceDeletedMessage: {e}", exc_info=True)

    async def start(self):
        """Starts the message consumer."""
        try:
            await self._connect()
            await self._declare_exchange()
            await self._setup_queue()

            if self.queue:
                # For simplicity, we'll use a single queue and dispatch internally based on routing_key.

                # Re-setup queue consumption for delete messages, as consume blocks.
                # Better approach: check message.routing_key inside _on_message_added
                # and call corresponding handler or ensure each bind has its own consumer.
                # For now, let's just use one consume and dispatch within _on_message_added
                # based on the routing key of the message itself.

                # Aio-pika's `consume` method takes a callback that will be called for all messages
                # on the queue. The routing key is available in `message.routing_key`.
                # We need a single main consumer loop that dispatches to _on_message_added or _on_message_deleted
                # based on the routing key.
                logger.info("Starting consuming messages...")
                await self.queue.consume(self._dispatch_message, no_ack=False)
                # Keep the event loop running
                # This will be run by FastAPI's startup event, so no need for asyncio.Future()
            else:
                logger.error("Queue not initialized. Consumer will not start.")
        except Exception as e:
            logger.error(f"Failed to start message consumer: {e}", exc_info=True)
            if self.connection:
                await self.connection.close()

    async def _dispatch_message(self, message: aio_pika.abc.AbstractIncomingMessage):
        """Dispatches messages to appropriate handlers based on routing key."""
        if message.routing_key == MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED:
            await self._on_message_added(message)
        elif message.routing_key == MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED:
            await self._on_message_deleted(message)
        else:
            logger.warning(f"Received message with unhandled routing key: {message.routing_key}. Body: {message.body.decode()}")
            await message.ack()  # Acknowledge unhandled messages to remove them from the queue

    async def stop(self):
        """Closes the RabbitMQ connection gracefully."""
        if self.connection:
            logger.info("Closing RabbitMQ connection...")
            await self.connection.close()
            logger.info("RabbitMQ connection closed.")
