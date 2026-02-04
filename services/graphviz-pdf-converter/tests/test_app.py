import pytest
import pika
import json
import subprocess
import os
import sys
from unittest.mock import MagicMock, patch, call
import signal # Added import

# Import functions from the app.py file
# We need to adjust the path to import app.py correctly
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
import app
sys.path.pop(0)

# Mock environment variables for consistent testing
@pytest.fixture(autouse=True)
def mock_env_vars():
    with patch.dict(os.environ, {
        'RABBITMQ__HOSTNAME': 'mock_rabbitmq_host',
        'RABBITMQ__PORT': '5672',
        'RABBITMQ__USERNAME': 'mock_user',
        'RABBITMQ__PASSWORD': 'mock_password',
    }):
        yield

@pytest.fixture
def mock_pika_connection():
    with patch('pika.BlockingConnection') as mock_conn:
        mock_channel = MagicMock()
        mock_conn.return_value.channel.return_value = mock_channel
        # Mock channel.is_open to return True by default
        mock_channel.is_open = True
        yield mock_conn, mock_channel

@pytest.fixture(autouse=True)
def reset_app_globals():
    # Ensure global connection/channel are reset for each test
    app.connection = None
    app.channel = None
    yield

# --- Test establish_rabbitmq_connection ---
def test_establish_rabbitmq_connection_success(mock_pika_connection):
    mock_conn, mock_channel = mock_pika_connection
    
    with (
        patch('app.RABBITMQ_HOST', 'mock_rabbitmq_host'),
        patch('app.RABBITMQ_PORT', 5672),
        patch('app.RABBITMQ_USER', 'mock_user'),
        patch('app.RABBITMQ_PASS', 'mock_password'),
    ):
        result_channel = app.establish_rabbitmq_connection()
        
        mock_conn.assert_called_once_with(pika.ConnectionParameters(
            host='mock_rabbitmq_host',
            port=5672,
            credentials=pika.PlainCredentials('mock_user', 'mock_password'),
            heartbeat=600
        ))
        mock_channel.queue_declare.assert_any_call(queue=app.RENDER_REQUEST_QUEUE, durable=True)
        mock_channel.queue_declare.assert_any_call(queue=app.RENDER_RESPONSE_QUEUE, durable=True)
        assert result_channel == mock_channel
        assert app.connection is not None
        assert app.channel == mock_channel

def test_establish_rabbitmq_connection_failure():
    with patch('pika.BlockingConnection', side_effect=pika.exceptions.AMQPConnectionError("Connection failed")):
        with patch('sys.exit') as mock_exit:
            app.establish_rabbitmq_connection()
            mock_exit.assert_called_once_with(1)

# --- Test publish_response ---
@pytest.mark.parametrize("status, pdf_path, error_message, expected_payload", [
    ("success", "/path/to/doc.pdf", None, {"job_id": "job123", "status": "success", "pdf_path": "/path/to/doc.pdf"}),
    ("error", None, "Something went wrong", {"job_id": "job456", "status": "error", "error_message": "Something went wrong"}),
    ("pending", None, None, {"job_id": "job789", "status": "pending"}),
])
def test_publish_response_success(mock_pika_connection, status, pdf_path, error_message, expected_payload):
    _, mock_channel = mock_pika_connection
    app.channel = mock_channel # Set global channel
    
    app.publish_response("job123" if status == "success" else "job456" if status == "error" else "job789", status, pdf_path, error_message)
    
    mock_channel.basic_publish.assert_called_once_with(
        exchange='',
        routing_key=app.RENDER_RESPONSE_QUEUE,
        body=json.dumps(expected_payload),
        properties=pika.BasicProperties(delivery_mode=2)
    )

def test_publish_response_reestablishes_connection(mock_pika_connection):
    mock_conn_obj, mock_channel_obj = mock_pika_connection # Rename to avoid confusion with patch target
    # Simulate channel being closed
    mock_channel_obj.is_open = False
    
    with (
        patch('app.establish_rabbitmq_connection') as mock_reconnect,
        patch('app.connection', new=mock_conn_obj.return_value, create=True), # Patch global app.connection
        patch('app.channel', new=mock_channel_obj, create=True) # Patch global app.channel
    ):
        mock_reconnect.return_value = mock_channel_obj # Mocked reconnection returns the channel object
        app.publish_response("job_reconnect", "pending")
        mock_reconnect.assert_called_once()
        mock_channel_obj.basic_publish.assert_called_once()

# --- Test ensure_output_directory ---
def test_ensure_output_directory():
    with patch('os.makedirs') as mock_makedirs:
        app.ensure_output_directory()
        mock_makedirs.assert_called_once_with(app.OUTPUT_DIR, exist_ok=True)

# --- Test process_render_request ---
@pytest.fixture
def mock_processing_dependencies():
    with (
        patch('os.path.exists') as mock_exists,
        patch('os.makedirs') as mock_makedirs,
        patch('subprocess.run') as mock_subprocess_run,
        patch('app.publish_response') as mock_publish_response
    ):
        
        # Default mocks for happy path
        mock_exists.return_value = True # Input file exists
        mock_subprocess_run.return_value = MagicMock(returncode=0, stdout="dot output", stderr="")
        
        mock_channel = MagicMock()
        mock_method = MagicMock(delivery_tag=1)

        yield mock_channel, mock_method, mock_exists, mock_makedirs, mock_subprocess_run, mock_publish_response

def test_process_render_request_success(mock_processing_dependencies):
    mock_channel, mock_method, mock_exists, _, mock_subprocess_run, mock_publish_response = mock_processing_dependencies
    
    body = json.dumps({
        "job_id": "test_job_id",
        "dot_filename": "test.dot",
        "page_size": "A0",
        "direction": "LR"
    }).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_exists.assert_called_once_with(os.path.join(app.INPUT_DIR, "test.dot"))
    mock_subprocess_run.assert_called_once()
    args, kwargs = mock_subprocess_run.call_args
    assert args[0][0] == 'dot'
    assert f"-sA0" in args[0]
    assert f"-Grankdir=LR" in args[0]
    assert os.path.join(app.INPUT_DIR, "test.dot") in args[0]
    assert os.path.join(app.OUTPUT_DIR, "test_job_id.pdf") in args[0]

    mock_publish_response.assert_called_once_with(
        "test_job_id", "success", pdf_path=os.path.join(app.OUTPUT_DIR, "test_job_id.pdf")
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_missing_job_id(mock_processing_dependencies):
    mock_channel, mock_method, _, _, _, mock_publish_response = mock_processing_dependencies
    body = json.dumps({"dot_filename": "test.dot"}).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        None, "error", error_message="Request validation failed for job None: Missing 'job_id' or 'dot_filename' in request."
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_dot_file_not_found(mock_processing_dependencies):
    mock_channel, mock_method, mock_exists, _, _, mock_publish_response = mock_processing_dependencies
    mock_exists.return_value = False # Simulate file not found
    body = json.dumps({"job_id": "job_nf", "dot_filename": "non_existent.dot"}).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        "job_nf", "error", error_message=f"Input .dot file not found: {os.path.join(app.INPUT_DIR, 'non_existent.dot')}"
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_subprocess_failure(mock_processing_dependencies):
    mock_channel, mock_method, _, _, mock_subprocess_run, mock_publish_response = mock_processing_dependencies
    mock_subprocess_run.return_value = MagicMock(returncode=1, stdout="err_stdout", stderr="err_stderr")
    body = json.dumps({"job_id": "job_fail", "dot_filename": "fail.dot"}).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once() # Check args in detail if needed
    args, kwargs = mock_publish_response.call_args
    assert args[0] == "job_fail"
    assert args[1] == "error"
    assert "Return code: 1" in kwargs['error_message']
    assert "Stdout: err_stdout" in kwargs['error_message']
    assert "Stderr: err_stderr" in kwargs['error_message']
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_subprocess_timeout(mock_processing_dependencies):
    mock_channel, mock_method, _, _, mock_subprocess_run, mock_publish_response = mock_processing_dependencies
    mock_subprocess_run.side_effect = subprocess.TimeoutExpired(cmd=['dot'], timeout=app.RENDER_TIMEOUT_SECONDS)
    body = json.dumps({"job_id": "job_timeout", "dot_filename": "timeout.dot"}).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        "job_timeout", "error", error_message=f"Graphviz command timed out after {app.RENDER_TIMEOUT_SECONDS} seconds for job job_timeout."
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_subprocess_command_not_found(mock_processing_dependencies):
    mock_channel, mock_method, _, _, mock_subprocess_run, mock_publish_response = mock_processing_dependencies
    mock_subprocess_run.side_effect = FileNotFoundError("dot not found")
    body = json.dumps({"job_id": "job_nofound", "dot_filename": "nofound.dot"}).encode('utf-8')
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_called_once_with(
        "job_nofound", "error", error_message="Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"
    )
    mock_channel.basic_ack.assert_called_once_with(1)

def test_process_render_request_invalid_json(mock_processing_dependencies):
    mock_channel, mock_method, _, _, _, mock_publish_response = mock_processing_dependencies
    body = b"invalid json"
    
    app.process_render_request(mock_channel, mock_method, None, body)
    
    mock_publish_response.assert_not_called() # No job_id to publish to
    mock_channel.basic_ack.assert_called_once_with(1) # Message should still be acked

# --- Test graceful_shutdown ---
def test_graceful_shutdown(mock_pika_connection):
    mock_conn, mock_channel = mock_pika_connection
    app.connection = mock_conn.return_value
    app.channel = mock_channel
    
    with patch('sys.exit') as mock_exit:
        app.graceful_shutdown(signal.SIGTERM, None)
        
        mock_channel.stop_consuming.assert_called_once()
        app.connection.close.assert_called_once()
        mock_exit.assert_called_once_with(0)

# --- Test main function ---
def test_main_starts_consumer(mock_pika_connection):
    mock_conn, mock_channel = mock_pika_connection
    with (
        patch('app.establish_rabbitmq_connection', return_value=mock_channel),
        patch('app.ensure_output_directory') as mock_ensure_dir,
        patch('app.logger') as mock_logger, # Mock logger to prevent actual output during test
    ):
        
        app.main()
        
        mock_ensure_dir.assert_called_once()
        mock_channel.basic_qos.assert_called_once_with(prefetch_count=1)
        mock_channel.basic_consume.assert_called_once_with(queue=app.RENDER_REQUEST_QUEUE, on_message_callback=app.process_render_request)
        mock_channel.start_consuming.assert_called_once()

def test_main_handles_keyboard_interrupt(mock_pika_connection):
    _, mock_channel = mock_pika_connection
    with (
        patch('app.establish_rabbitmq_connection', return_value=mock_channel),
        patch('app.graceful_shutdown') as mock_graceful_shutdown,
        patch('app.ensure_output_directory') # Mock ensure_output_directory
    ):
        
        mock_channel.start_consuming.side_effect = KeyboardInterrupt
        
        app.main()
        mock_graceful_shutdown.assert_called_once()

def test_main_handles_connection_closed_by_broker(mock_pika_connection):
    _, mock_channel = mock_pika_connection
    # Make the first call to start_consuming raise ConnectionClosedByBroker
    # and the subsequent one (from recursive main call) work
    mock_channel.start_consuming.side_effect = [pika.exceptions.ConnectionClosedByBroker(reply_code=200, reply_text="Test close"), None]
    
    with (
        patch('app.establish_rabbitmq_connection', return_value=mock_channel),
        patch('app.main', wraps=app.main) as mock_main_wrapped,
        patch('time.sleep') as mock_sleep,
        patch('app.ensure_output_directory') # Mock ensure_output_directory
    ):
        
        # Call the original main once, it will recurse
        mock_main_wrapped()
        
        # Called once for the initial call, then once more for the retry
        assert mock_main_wrapped.call_count == 2 
        mock_sleep.assert_called_once_with(5)
