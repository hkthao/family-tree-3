import pytest
import pika
import json
import subprocess
import os
import sys
from unittest.mock import MagicMock, patch, call
from pydantic import ValidationError
import signal # Added import

# Adjust path to import modules correctly
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..')))
from src.messaging import consumer
from src.core.config import config
from src.core.domain import RenderRequest, RenderResponse
from src.core.services import GraphvizService
sys.path.pop(0)

# Mock environment variables for consistent testing
@pytest.fixture(autouse=True)
def mock_config_settings():
    with (
        patch.object(config, 'RABBITMQ_HOST', 'mock_rabbitmq_host'),
        patch.object(config, 'RABBITMQ_PORT', 5672),
        patch.object(config, 'RABBITMQ_USER', 'mock_user'),
        patch.object(config, 'RABBITMQ_PASS', 'mock_password'),
        patch.object(config, 'RABBITMQ_HEARTBEAT', 600),
        patch.object(config, 'INPUT_DIR', '/mock/input'),
        patch.object(config, 'OUTPUT_DIR', '/mock/output'),
        patch.object(config, 'RENDER_REQUEST_QUEUE', 'mock_render_request'),
        patch.object(config, 'RENDER_REQUEST_EXCHANGE', 'mock_render_request_exchange'),
        patch.object(config, 'STATUS_UPDATE_EXCHANGE', 'mock_status_update_exchange'),
        patch.object(config, 'STATUS_UPDATE_ROUTING_KEY', 'mock_status.updated'),
        patch.object(config, 'RENDER_TIMEOUT_SECONDS', 5) # Shorter timeout for tests
    ):
        yield

@pytest.fixture
def mock_pika_connection():
    with patch('pika.BlockingConnection') as mock_conn_cls:
        mock_connection_instance = MagicMock()
        mock_channel_instance = MagicMock()
        mock_connection_instance.channel.return_value = mock_channel_instance
        mock_channel_instance.is_open = True
        mock_conn_cls.return_value = mock_connection_instance
        yield mock_conn_cls, mock_channel_instance # Yield mock_conn_cls (the class mock) and the channel instance

# --- Test establish_rabbitmq_connection ---
def test_establish_rabbitmq_connection_success(mock_pika_connection):
    mock_conn_cls, mock_channel = mock_pika_connection
    
    connection, channel = consumer.establish_rabbitmq_connection()
    
    mock_conn_cls.assert_called_once_with(pika.ConnectionParameters(
        host='mock_rabbitmq_host',
        port=5672,
        credentials=pika.PlainCredentials('mock_user', 'mock_password'),
        heartbeat=600
    ))
    mock_channel.queue_declare.assert_any_call(queue='mock_render_request', durable=True)
    mock_channel.exchange_declare.assert_any_call(exchange='mock_render_request_exchange', exchange_type='topic', durable=True)
    mock_channel.queue_bind.assert_called_with(
        queue='mock_render_request',
        exchange='mock_render_request_exchange',
        routing_key='mock_render_request'
    )
    mock_channel.exchange_declare.assert_any_call(exchange='mock_status_update_exchange', exchange_type='topic', durable=True)
    assert connection == mock_conn_cls.return_value
    assert channel == mock_channel

def test_establish_rabbitmq_connection_failure():
    with patch('pika.BlockingConnection', side_effect=pika.exceptions.AMQPConnectionError("Connection failed")), \
         patch('sys.exit') as mock_exit:
        consumer.establish_rabbitmq_connection()
        mock_exit.assert_called_once_with(1)

# --- Test publish_response ---
@pytest.mark.parametrize("status_val, output_path, error_msg", [
    ("success", "/path/to/doc.pdf", None),
    ("error", None, "Something went wrong"),
])
def test_publish_response_success(mock_pika_connection, status_val, output_path, error_msg):
    _, mock_channel = mock_pika_connection
    job_id = "job123"
    
    consumer.publish_response(mock_channel, job_id, status_val, output_path, error_msg)
    
    expected_payload = RenderResponse(
        job_id=job_id,
        status=status_val,
        output_file_path=output_path,
        error_message=error_msg
    ).model_dump_json()

    mock_channel.basic_publish.assert_called_once_with(
        exchange='mock_status_update_exchange',
        routing_key='mock_status.updated',
        body=expected_payload,
        properties=pika.BasicProperties(delivery_mode=2)
    )

def test_publish_response_channel_closed(mock_pika_connection):
    _, mock_channel = mock_pika_connection
    mock_channel.is_open = False
    
    consumer.publish_response(mock_channel, "job_closed", "pending")
    
    mock_channel.basic_publish.assert_not_called()

# --- Test ensure_output_directory ---
def test_ensure_output_directory():
    with patch('os.makedirs') as mock_makedirs:
        consumer.ensure_output_directory()
        mock_makedirs.assert_called_once_with('/mock/output', exist_ok=True)

# --- Test process_render_request ---
@pytest.fixture
def mock_consumer_dependencies():
    with (
        patch('src.messaging.consumer.ensure_output_directory'),
        patch('src.messaging.consumer.publish_response') as mock_publish_response,
        patch('src.core.services.GraphvizService.render_dot_to_pdf') as mock_render_dot_to_pdf
    ):
        mock_channel = MagicMock(is_open=True)
        mock_method = MagicMock(delivery_tag=1)
        yield mock_channel, mock_method, mock_publish_response, mock_render_dot_to_pdf

def test_process_render_request_success(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, mock_render_dot_to_pdf = mock_consumer_dependencies
    
    mock_render_dot_to_pdf.return_value = '/mock/output/test_job_id.pdf'
    body = RenderRequest(
        job_id="test_job_id",
        dot_filename="test.dot",
        page_size="A0",
        direction="LR"
    ).model_dump_json().encode('utf-8')
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    mock_render_dot_to_pdf.assert_called_once()
    args, kwargs = mock_render_dot_to_pdf.call_args
    assert isinstance(args[0], RenderRequest)
    assert args[0].job_id == "test_job_id"

    mock_publish_response.assert_called_once_with(
        mock_channel, "test_job_id", "success", output_file_path='/mock/output/test_job_id.pdf'
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_dot_file_not_found(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, mock_render_dot_to_pdf = mock_consumer_dependencies
    
    mock_render_dot_to_pdf.side_effect = FileNotFoundError("Input .dot file not found: /mock/input/non_existent.dot")
    body = RenderRequest(
        job_id="job_nf",
        dot_filename="non_existent.dot"
    ).model_dump_json().encode('utf-8')
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        mock_channel, "job_nf", "error", 
        error_message="Graphviz rendering failed for job job_nf: Input .dot file not found: /mock/input/non_existent.dot"
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_subprocess_failure(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, mock_render_dot_to_pdf = mock_consumer_dependencies
    
    mock_render_dot_to_pdf.side_effect = RuntimeError("Graphviz command failed")
    body = RenderRequest(
        job_id="job_fail",
        dot_filename="fail.dot"
    ).model_dump_json().encode('utf-8')
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        mock_channel, "job_fail", "error", 
        error_message="Graphviz rendering failed for job job_fail: Graphviz command failed"
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_subprocess_timeout(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, mock_render_dot_to_pdf = mock_consumer_dependencies
    
    mock_render_dot_to_pdf.side_effect = subprocess.TimeoutExpired(cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS)
    body = RenderRequest(
        job_id="job_timeout",
        dot_filename="timeout.dot"
    ).model_dump_json().encode('utf-8')
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        mock_channel, "job_timeout", "error", 
        error_message=f"Graphviz rendering failed for job job_timeout: Command '['dot']' timed out after {config.RENDER_TIMEOUT_SECONDS} seconds"
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_invalid_json(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, _ = mock_consumer_dependencies
    body = b"invalid json"
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_not_called()
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_validation_error(mock_consumer_dependencies):
    mock_channel, mock_method, mock_publish_response, _ = mock_consumer_dependencies
    body = json.dumps({"dot_filename": "missing_job_id.dot"}).encode('utf-8') # Missing job_id
    
    consumer.process_render_request(mock_channel, mock_method, None, body)
    
    # Check for "job_id" and "Field required" (case-insensitive) in the error message
    error_message_detail = mock_publish_response.call_args[1]['error_message']
    assert "job_id" in error_message_detail
    assert "field required" in error_message_detail.lower()

    mock_channel.basic_ack.assert_called_once_with(1)


# --- Test graceful_shutdown ---
def test_graceful_shutdown(mock_pika_connection):
    mock_conn_cls, mock_channel = mock_pika_connection
    mock_conn = mock_conn_cls.return_value # Get the instance mock
    
    with patch('sys.exit') as mock_exit:
        consumer.graceful_shutdown(signal.SIGTERM, None, mock_conn, mock_channel)
        
        mock_channel.stop_consuming.assert_called_once()
        mock_conn.close.assert_called_once()
        mock_exit.assert_called_once_with(0)

def test_graceful_shutdown_no_active_connection():
    with patch('sys.exit') as mock_exit, \
         patch('src.messaging.consumer.logger') as mock_logger:
        consumer.graceful_shutdown(signal.SIGTERM, None, None, None)
        # Assert that info was called with "SIGTERM received..." at some point
        mock_logger.info.assert_any_call("SIGTERM received, initiating graceful shutdown...")
        mock_logger.info.assert_called_with("Shutdown complete.") # This is the last call
        mock_exit.assert_called_once_with(0)


# --- Test main function ---
def test_main_starts_consumer(mock_pika_connection):
    mock_conn_cls, mock_channel = mock_pika_connection
    mock_conn = mock_conn_cls.return_value # Get the instance mock
    with (
        patch('src.messaging.consumer.establish_rabbitmq_connection', return_value=(mock_conn, mock_channel)),
        patch('src.messaging.consumer.ensure_output_directory'),
        patch('src.messaging.consumer.logger'),
        patch('src.messaging.consumer.signal.signal'),
        patch.object(mock_channel, 'start_consuming') # Patch start_consuming to prevent infinite loop
    ):
        consumer.main()
        
        consumer.ensure_output_directory.assert_called_once()
        consumer.establish_rabbitmq_connection.assert_called_once()
        mock_channel.basic_qos.assert_called_once_with(prefetch_count=1)
        mock_channel.basic_consume.assert_called_once()
        assert mock_channel.basic_consume.call_args[1]['queue'] == config.RENDER_REQUEST_QUEUE
        # The on_message_callback is a lambda, so checking its presence is sufficient
        assert mock_channel.basic_consume.call_args[1]['on_message_callback'] is not None
        mock_channel.start_consuming.assert_called_once()

def test_main_handles_keyboard_interrupt(mock_pika_connection):
    mock_conn_cls, mock_channel = mock_pika_connection
    mock_conn = mock_conn_cls.return_value
    with (
        patch('src.messaging.consumer.establish_rabbitmq_connection', return_value=(mock_conn, mock_channel)),
        patch('src.messaging.consumer.graceful_shutdown') as mock_graceful_shutdown,
        patch('src.messaging.consumer.ensure_output_directory'),
        patch.object(mock_channel, 'start_consuming', side_effect=KeyboardInterrupt)
    ):
        consumer.main()
        mock_graceful_shutdown.assert_called_once_with(None, None, mock_conn, mock_channel)

def test_main_handles_connection_closed_by_broker():
    # Simulate connection closed by broker, leading to a retry
    with (
        patch('src.messaging.consumer.establish_rabbitmq_connection') as mock_establish,
        patch('src.messaging.consumer.ensure_output_directory'),
        patch('src.messaging.consumer.time.sleep') as mock_sleep,
        patch('src.messaging.consumer.main', wraps=consumer.main) as mock_main_wrapped, # Wraps to allow actual calls
        patch('sys.exit') # Mock sys.exit to prevent test runner from exiting
    ):
        # Configure mock_establish to return a valid connection/channel on its second call
        mock_conn_1_cls = MagicMock()
        mock_channel_1 = MagicMock()
        mock_conn_1_instance = mock_conn_1_cls.return_value
        
        mock_conn_2_cls = MagicMock()
        mock_channel_2 = MagicMock()
        mock_conn_2_instance = mock_conn_2_cls.return_value

        mock_channel_1.start_consuming.side_effect = pika.exceptions.ConnectionClosedByBroker(reply_code=200, reply_text="Test close")
        mock_channel_2.start_consuming.side_effect = KeyboardInterrupt # Stop the second main call from looping
        
        mock_establish.side_effect = [(mock_conn_1_instance, mock_channel_1), (mock_conn_2_instance, mock_channel_2)]

        # Call the original main once, it will recurse
        mock_main_wrapped()
        
        # main() should be called twice (initial + retry)
        assert mock_main_wrapped.call_count == 2 
        mock_sleep.assert_called_once_with(5)
        # Ensure that start_consuming was called on both mock channels
        mock_channel_1.start_consuming.assert_called_once()
        mock_channel_2.start_consuming.assert_called_once()