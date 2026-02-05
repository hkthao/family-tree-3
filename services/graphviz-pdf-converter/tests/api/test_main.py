import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock
import os
import sys
import subprocess

# Adjust path to import modules correctly
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..')))
from src.api.main import app
from src.core.domain import RenderRequest, RenderResponse
from src.core.config import config
sys.path.pop(0)

client = TestClient(app)

# Mock config settings
@pytest.fixture(autouse=True)
def mock_config_settings():
    with (
        patch.object(config, 'INPUT_DIR', '/mock/input'),
        patch.object(config, 'OUTPUT_DIR', '/mock/output'),
        patch.object(config, 'RENDER_TIMEOUT_SECONDS', 5) # Shorter timeout for tests
    ):
        yield

@pytest.fixture
def mock_render_request_data():
    return {
        "job_id": "api_test_job",
        "dot_filename": "api_test.dot",
        "page_size": "Letter",
        "direction": "TB"
    }

class TestFastAPI:

    def test_root_endpoint(self):
        response = client.get("/")
        assert response.status_code == 200
        assert response.json() == {"message": "Graphviz PDF Converter API is running."}

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_success(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_render_dot_to_pdf.return_value = os.path.join(config.OUTPUT_DIR, f"{mock_render_request_data['job_id']}.pdf")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 200
        expected_response = RenderResponse(
            job_id=mock_render_request_data['job_id'],
            status="success",
            output_file_path=mock_render_dot_to_pdf.return_value
        )
        assert response.json() == expected_response.model_dump()
        mock_render_dot_to_pdf.assert_called_once()
        args, kwargs = mock_render_dot_to_pdf.call_args
        assert isinstance(args[0], RenderRequest)
        assert args[0].job_id == mock_render_request_data['job_id']

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_input_file_not_found(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Input .dot file not found: /mock/input/non_existent.dot")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 404
        assert response.json() == {"detail": "Input DOT file not found: Input .dot file not found: /mock/input/non_existent.dot"}
        mock_render_dot_to_pdf.assert_called_once()

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_dot_command_not_found(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Graphviz 'dot' command not found. Is Graphviz installed and in PATH?")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "Graphviz 'dot' command not found on server: Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"}
        mock_render_dot_to_pdf.assert_called_once()

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_timeout(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_exception = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b'timed out'
        )
        mock_render_dot_to_pdf.side_effect = mock_exception

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 504
        assert response.json() == {"detail": f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {mock_exception.stderr.decode().strip()}"}

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_command_failure(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_render_dot_to_pdf.side_effect = RuntimeError("Graphviz command failed: error message")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "Graphviz rendering failed: Graphviz command failed: error message"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_invalid_request_body(self, mock_render_request_data):
        invalid_data = mock_render_request_data.copy()
        invalid_data.pop("job_id") # Missing required field

        response = client.post("/render", json=invalid_data)

        assert response.status_code == 422 # Unprocessable Entity for Pydantic validation errors
        assert "job_id" in response.json()["detail"][0]["loc"]
        assert "field required".lower() in response.json()["detail"][0]["msg"].lower()

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    def test_render_unexpected_error(self, mock_render_dot_to_pdf, mock_render_request_data):
        mock_render_dot_to_pdf.side_effect = Exception("An unforeseen issue occurred")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "An unexpected server error occurred: An unforeseen issue occurred"}
        mock_render_dot_to_pdf.assert_called_once()
