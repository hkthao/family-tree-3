import pytest
import asyncio
import json
from unittest.mock import AsyncMock, MagicMock, patch

import aio_pika

from app.message_consumer import MessageConsumer, RABBITMQ_URL
from app.services.face_service import FaceService
from app.models import (
    MemberFaceAddedMessage,
    MemberFaceDeletedMessage,
    MessageBusConstants,
    FaceAddRequestModel,
    MetadataModel,
    BoundingBoxModel,
)


@pytest.fixture
def mock_face_service():
    """Fixture for a mocked FaceService instance."""
    mock = MagicMock(spec=FaceService)
    mock.add_face_by_vector = AsyncMock()
    mock.delete_face = AsyncMock(return_value=True)
    return mock


@pytest.fixture
def mock_aio_pika_connection():
    """Mocks aio_pika.connect_robust and related objects."""
    with patch("aio_pika.connect_robust", new_callable=AsyncMock) as mock_connect_robust:
        mock_connection = AsyncMock(spec=aio_pika.RobustConnection)
        mock_channel = AsyncMock(spec=aio_pika.RobustChannel)
        mock_exchange = AsyncMock(spec=aio_pika.Exchange)
        mock_queue = AsyncMock(spec=aio_pika.Queue)

        mock_connect_robust.return_value = mock_connection
        mock_connection.channel = AsyncMock(return_value=mock_channel)
        mock_channel.declare_exchange.return_value = mock_exchange
        mock_channel.declare_queue.return_value = mock_queue

        mock_connection.close = AsyncMock() # Ensure close is awaitable

        yield mock_connect_robust, mock_connection, mock_channel, mock_exchange, mock_queue


@pytest.fixture
def message_consumer_instance(mock_face_service):
    """Fixture for a MessageConsumer instance with mocked FaceService."""
    return MessageConsumer(face_service=mock_face_service)


@pytest.mark.asyncio
async def test_consumer_connect(message_consumer_instance, mock_aio_pika_connection):
    """Test that the consumer connects to RabbitMQ."""
    mock_connect_robust, _, _, _, _ = mock_aio_pika_connection
    await message_consumer_instance._connect()
    mock_connect_robust.assert_called_once_with(RABBITMQ_URL)
    assert message_consumer_instance.connection is not None
    assert message_consumer_instance.channel is not None


@pytest.mark.asyncio
async def test_consumer_declare_exchange(message_consumer_instance, mock_aio_pika_connection):
    """Test that the consumer declares the exchange."""
    _, mock_connection, mock_channel, mock_exchange, _ = mock_aio_pika_connection
    await message_consumer_instance._connect() # Establish connection first
    await message_consumer_instance._declare_exchange()
    mock_channel.declare_exchange.assert_called_once_with(
        MessageBusConstants.Exchanges.MEMBER_FACE, aio_pika.ExchangeType.TOPIC, durable=True
    )
    assert message_consumer_instance.exchange == mock_exchange


@pytest.mark.asyncio
async def test_consumer_setup_queue(message_consumer_instance, mock_aio_pika_connection):
    """Test that the consumer sets up and binds the queue."""
    _, mock_connection, mock_channel, mock_exchange, mock_queue = mock_aio_pika_connection
    await message_consumer_instance._connect()
    message_consumer_instance.exchange = mock_exchange # Manually set exchange for testing
    await message_consumer_instance._setup_queue()

    mock_channel.declare_queue.assert_called_once()
    mock_queue.bind.assert_any_call(mock_exchange, MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED)
    mock_queue.bind.assert_any_call(mock_exchange, MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED)
    assert mock_queue.bind.call_count == 2
    assert message_consumer_instance.queue == mock_queue


@pytest.mark.asyncio
async def test_on_message_added_success(message_consumer_instance, mock_face_service):
    """Test processing of a valid MemberFaceAddedMessage."""
    bounding_box_data = BoundingBoxModel(X=10.0, Y=20.0, Width=30.0, Height=40.0)
    metadata_data = MetadataModel(
        FamilyId="family1",
        MemberId="member1",
        FaceId="face1",
        BoundingBox=bounding_box_data,
        Confidence=0.95,
        ThumbnailUrl="http://thumb.url",
        OriginalImageUrl="http://orig.url",
        Emotion="happy",
        EmotionConfidence=0.99,
    )
    face_add_request_data = FaceAddRequestModel(Vector=[0.1] * 128, Metadata=metadata_data)
    message_body = MemberFaceAddedMessage(
        FaceAddRequest=face_add_request_data, MemberFaceLocalId="local123"
    ).model_dump_json().encode()

    mock_message = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message.body = message_body
    mock_message.routing_key = MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED

    await message_consumer_instance._on_message_added(mock_message)

    expected_metadata_for_service = {
        "FamilyId": "family1",
        "MemberId": "member1",
        "FaceId": "face1",
        "BoundingBox": {"X": 10, "Y": 20, "Width": 30, "Height": 40}, # Ensure int conversion
        "Confidence": 0.95,
        "ThumbnailUrl": "http://thumb.url",
        "OriginalImageUrl": "http://orig.url",
        "Emotion": "happy",
        "EmotionConfidence": 0.99,
        "faceId": "face1", # Ensure faceId is explicitly added/mapped
    }
    mock_face_service.add_face_by_vector.assert_called_once_with(
        [0.1] * 128, expected_metadata_for_service
    )
    mock_message.process.assert_called_once()


@pytest.mark.asyncio
async def test_on_message_deleted_success(message_consumer_instance, mock_face_service):
    """Test processing of a valid MemberFaceDeletedMessage."""
    message_body = MemberFaceDeletedMessage(
        MemberFaceId="local_del_id",
        VectorDbId="qdrant_face_id",
        MemberId="member_del_id",
        FamilyId="family_del_id",
    ).model_dump_json().encode()

    mock_message = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message.body = message_body
    mock_message.routing_key = MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED

    await message_consumer_instance._on_message_deleted(mock_message)

    mock_face_service.delete_face.assert_called_once_with("qdrant_face_id")
    mock_message.process.assert_called_once()


@pytest.mark.asyncio
async def test_on_message_deleted_no_vector_db_id(message_consumer_instance, mock_face_service):
    """Test processing of MemberFaceDeletedMessage when VectorDbId is null."""
    message_body = MemberFaceDeletedMessage(
        MemberFaceId="local_del_id",
        VectorDbId=None,
        MemberId="member_del_id",
        FamilyId="family_del_id",
    ).model_dump_json().encode()

    mock_message = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message.body = message_body
    mock_message.routing_key = MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED

    await message_consumer_instance._on_message_deleted(mock_message)

    mock_face_service.delete_face.assert_not_called()
    mock_message.process.assert_called_once()


@pytest.mark.asyncio
async def test_dispatch_message(message_consumer_instance):
    """Test that messages are dispatched to the correct handler."""
    mock_message_added = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message_added.routing_key = MessageBusConstants.RoutingKeys.MEMBER_FACE_ADDED
    mock_message_added.body = b"{}" # Dummy body

    mock_message_deleted = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message_deleted.routing_key = MessageBusConstants.RoutingKeys.MEMBER_FACE_DELETED
    mock_message_deleted.body = b"{}" # Dummy body

    mock_message_unhandled = AsyncMock(spec=aio_pika.abc.AbstractIncomingMessage)
    mock_message_unhandled.routing_key = "unhandled.key"
    mock_message_unhandled.body = b"{}" # Dummy body

    with patch.object(message_consumer_instance, '_on_message_added', new_callable=AsyncMock) as mock_on_added, \
         patch.object(message_consumer_instance, '_on_message_deleted', new_callable=AsyncMock) as mock_on_deleted:
        
        await message_consumer_instance._dispatch_message(mock_message_added)
        mock_on_added.assert_called_once_with(mock_message_added)
        mock_on_deleted.assert_not_called()
        mock_message_added.ack.assert_not_called() # Should be processed by handler

        mock_on_added.reset_mock()
        mock_on_deleted.reset_mock()

        await message_consumer_instance._dispatch_message(mock_message_deleted)
        mock_on_deleted.assert_called_once_with(mock_message_deleted)
        mock_on_added.assert_not_called()
        mock_message_deleted.ack.assert_not_called() # Should be processed by handler

        mock_on_added.reset_mock()
        mock_on_deleted.reset_mock()

        await message_consumer_instance._dispatch_message(mock_message_unhandled)
        mock_on_added.assert_not_called()
        mock_on_deleted.assert_not_called()
        mock_message_unhandled.ack.assert_called_once() # Unhandled messages should be acknowledged


@pytest.mark.asyncio
async def test_consumer_start_stop(message_consumer_instance, mock_aio_pika_connection):
    """Test the start and stop functionality of the consumer."""
    mock_connect_robust, mock_connection, mock_channel, mock_exchange, mock_queue = mock_aio_pika_connection

    # Mock the consume method to avoid blocking during test
    # We want to ensure it's called, but not run the actual consumer loop
    mock_queue.consume = MagicMock(return_value=asyncio.Future())
    mock_queue.consume.return_value.set_result(None) # Make the future immediately resolve

    # Start the consumer
    await message_consumer_instance.start()

    mock_connect_robust.assert_called_once()
    mock_channel.declare_exchange.assert_called_once()
    mock_channel.declare_queue.assert_called_once()
    mock_queue.bind.call_count == 2
    mock_queue.consume.assert_called_once_with(message_consumer_instance._dispatch_message, no_ack=False)

    # Stop the consumer
    await message_consumer_instance.stop()
    mock_connection.close.assert_called_once()
