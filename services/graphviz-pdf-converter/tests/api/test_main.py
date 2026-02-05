import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock
import os
import sys
import subprocess
import stat # Added import

# Adjust path to import modules correctly
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..')))
from src.api.main import app
from src.core.domain import RenderRequest, RenderResponse
from src.core.config import config
sys.path.pop(0)

client = TestClient(app)

# Mock config settings
@pytest.fixture
def mock_config_settings(tmp_path):
    input_dir = tmp_path / "mock_input"
    output_dir = tmp_path / "mock_output"
    input_dir.mkdir()
    output_dir.mkdir()
    with (
        patch.object(config, 'INPUT_DIR', str(input_dir)),
        patch.object(config, 'OUTPUT_DIR', str(output_dir)),
        patch.object(config, 'RENDER_TIMEOUT_SECONDS', 5) # Shorter timeout for tests
    ):
        yield input_dir, output_dir # Yield the created directories for potential use in tests

@pytest.fixture
def mock_render_request_data():
    return {
        "job_id": "api_test_job",
        "dot_filename": "api_test.dot",
        "page_size": "Letter",
        "direction": "TB"
    }

@pytest.fixture
def mock_pdf_file(mock_config_settings):
    """
    Creates a dummy PDF file in the mocked output directory and returns its path.
    Cleans up the file after the test.
    """
    output_file_path = os.path.join(config.OUTPUT_DIR, "dummy_output.pdf")
    # os.makedirs(config.OUTPUT_DIR, exist_ok=True) # Directory will be created by mock_config_settings
    with open(output_file_path, "wb") as f:
        f.write(b"%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj 2 0 obj<</Type/Pages/Count 0>>endobj\nxref\n0 3\n0000000000 65535 f\n0000000009 00000 n\n0000000057 00000 n\ntrailer<</Size 3/Root 1 0 R>>startxref\n106\n%%EOF")
    yield output_file_path
    os.remove(output_file_path)

class TestFastAPI:

    def test_root_endpoint(self):
        response = client.get("/")
        assert response.status_code == 200
        assert response.json() == {"message": "Graphviz PDF Converter API is running."}

    def test_render_success(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
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

    def test_render_input_file_not_found(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Input .dot file not found: /mock/input/non_existent.dot")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 404
        assert response.json() == {"detail": "Input DOT file not found: Input .dot file not found: /mock/input/non_existent.dot"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_dot_command_not_found(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Graphviz 'dot' command not found. Is Graphviz installed and in PATH?")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "Graphviz 'dot' command not found on server: Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_timeout(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_exception = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b'timed out'
        )
        mock_render_dot_to_pdf.side_effect = mock_exception

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 504
        assert response.json() == {"detail": f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {mock_exception.stderr.decode().strip()}"}

    def test_render_command_failure(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
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

    def test_render_unexpected_error(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = Exception("An unforeseen issue occurred")

        response = client.post("/render", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "An unexpected server error occurred: An unforeseen issue occurred"}
        mock_render_dot_to_pdf.assert_called_once()

    # --- Tests for /render-pdf-filename endpoint ---
    def test_render_pdf_filename_success(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.return_value = os.path.join(config.OUTPUT_DIR, f"{mock_render_request_data['job_id']}.pdf")

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

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

    def test_render_pdf_filename_input_file_not_found(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Input .dot file not found: /mock/input/non_existent.dot")

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

        assert response.status_code == 404
        assert response.json() == {"detail": "Input DOT file not found: Input .dot file not found: /mock/input/non_existent.dot"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_pdf_filename_dot_command_not_found(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Graphviz 'dot' command not found. Is Graphviz installed and in PATH?")

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "Graphviz 'dot' command not found on server: Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_pdf_filename_timeout(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_exception = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b'timed out'
        )
        mock_render_dot_to_pdf.side_effect = mock_exception

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

        assert response.status_code == 504
        assert response.json() == {"detail": f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {mock_exception.stderr.decode().strip()}"}

    def test_render_pdf_filename_command_failure(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = RuntimeError("Graphviz command failed: error message")

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "Graphviz rendering failed: Graphviz command failed: error message"}
        mock_render_dot_to_pdf.assert_called_once()

    def test_render_pdf_filename_invalid_request_body(self, mock_render_request_data):
        invalid_data = mock_render_request_data.copy()
        invalid_data.pop("job_id") # Missing required field

        response = client.post("/render-pdf-filename", json=invalid_data)

        assert response.status_code == 422 # Unprocessable Entity for Pydantic validation errors
        assert "job_id" in response.json()["detail"][0]["loc"]
        assert "field required".lower() in response.json()["detail"][0]["msg"].lower()

    def test_render_pdf_filename_unexpected_error(self, mock_render_request_data, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = Exception("An unforeseen issue occurred")

        response = client.post("/render-pdf-filename", json=mock_render_request_data)

        assert response.status_code == 500
        assert response.json() == {"detail": "An unexpected server error occurred: An unforeseen issue occurred"}
        mock_render_dot_to_pdf.assert_called_once()


    # --- Tests for /render-and-download endpoint ---

    @patch('os.path.exists', return_value=True) # For FileResponse
    @patch('os.remove') # For cleanup
    @patch('os.stat') # Mock os.stat for FileResponse
    def test_render_and_download_file_upload_success(
        self, mock_os_stat, mock_os_remove, mock_os_path_exists, mock_pdf_file, mock_config_settings, mocker
    ):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf', return_value=mock_pdf_file)
        # Mock os.stat to return a stat_result object with necessary attributes
        mock_stat_result = MagicMock()
        mock_stat_result.st_size = 1234 # Example size
        mock_stat_result.st_mtime = 1678886400 # Example modification time
        mock_stat_result.st_mode = stat.S_IFREG | 0o777 # Indicate it's a regular file
        mock_os_stat.return_value = mock_stat_result

        # Use mocker for save_upload_file
        mock_save_upload_file = mocker.patch('src.api.main.save_upload_file', return_value="unique_filename.dot")
        
        import io
        with io.BytesIO(b"digraph G { A -> B; }") as f:
            response = client.post(
                "/render-and-download",
                files={"dot_file": ("my_uploaded_graph.dot", f, "text/plain")},
                data={
                    "job_id": "uploaded_job",
                    "page_size": "A4",
                    "direction": "TB"
                }
            )

        assert response.status_code == 200
        assert response.headers["content-type"] == "application/pdf"
        assert response.headers["content-disposition"] == 'attachment; filename="uploaded_job.pdf"'
        assert mock_render_dot_to_pdf.called
        mock_save_upload_file.assert_called_once()
        mock_os_remove.assert_called_once_with(os.path.join(config.INPUT_DIR, "unique_filename.dot")) # Only check for cleanup of the uploaded file

    @patch('src.core.services.GraphvizService.render_dot_to_pdf')
    @patch('os.path.exists', return_value=True) # For FileResponse
    @patch('os.stat') # Mock os.stat for FileResponse
    def test_render_and_download_path_success(self, mock_os_stat, mock_os_path_exists, mock_pdf_file, mock_config_settings, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf', return_value=mock_pdf_file)
        # Mock os.stat to return a stat_result object with necessary attributes
        mock_stat_result = MagicMock()
        mock_stat_result.st_size = 5678 # Example size
        mock_stat_result.st_mtime = 1678886400 # Example modification time
        mock_stat_result.st_mode = stat.S_IFREG | 0o777 # Indicate it's a regular file
        mock_os_stat.return_value = mock_stat_result

        response = client.post(
            "/render-and-download",
            data={
                "job_id": "path_job",
                "dot_filename_path": "my_existing_graph.dot",
                "page_size": "Letter",
                "direction": "LR"
            }
        )

        assert response.status_code == 200
        assert response.headers["content-type"] == "application/pdf"
        assert response.headers["content-disposition"] == 'attachment; filename="path_job.pdf"'
        assert mock_render_dot_to_pdf.called

    def test_render_and_download_missing_input(self):
        response = client.post(
            "/render-and-download",
            data={
                "job_id": "no_input_job"
            }
        )
        assert response.status_code == 400
        assert response.json() == {"detail": "Either 'dot_file' (file upload) or 'dot_filename_path' (path to existing .dot file) must be provided."}

    @patch('os.path.exists', return_value=True)
    @patch('os.stat') # Mock os.stat for FileResponse
    def test_render_and_download_file_not_found_error(self, mock_os_stat, mock_os_path_exists, mock_config_settings, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_os_stat.return_value = MagicMock(st_size=1, st_mtime=1) # Minimal mock for FileResponse to pass internal checks
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Input .dot file not found: /mock/input/non_existent.dot")
        response = client.post(
            "/render-and-download",
            data={
                "job_id": "job_nf",
                "dot_filename_path": "non_existent.dot"
            }
        )
        assert response.status_code == 404
        assert "Input DOT file not found" in response.json()["detail"]

    @patch('os.path.exists', return_value=True)
    @patch('os.stat') # Mock os.stat for FileResponse
    def test_render_and_download_timeout_error(self, mock_os_stat, mock_os_path_exists, mock_config_settings, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_os_stat.return_value = MagicMock(st_size=1, st_mtime=1) # Minimal mock for FileResponse
        mock_exception = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b'timed out'
        )
        mock_render_dot_to_pdf.side_effect = mock_exception
        response = client.post(
            "/render-and-download",
            data={
                "job_id": "job_timeout",
                "dot_filename_path": "timeout.dot"
            }
        )
        assert response.status_code == 504
        assert f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {mock_exception.stderr.decode().strip()}" in response.json()["detail"]

    @patch('os.path.exists', return_value=True)
    @patch('os.stat') # Mock os.stat for FileResponse
    def test_render_and_download_cleanup_on_error(
        self, mock_os_stat, mock_os_path_exists, mock_config_settings, mocker
    ):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_os_stat.return_value = MagicMock(st_size=1, st_mtime=1) # Minimal mock for FileResponse
        mock_render_dot_to_pdf.side_effect = RuntimeError("Rendering failed during cleanup test")
        
        # Patch os.remove to ensure only the cleanup in the finally block is counted
        mock_os_remove = mocker.patch('os.remove')
        
        # Use mocker for save_upload_file
        mock_save_upload_file = mocker.patch('src.api.main.save_upload_file', return_value=os.path.join(config.INPUT_DIR, "unique_filename.dot"))
        
        import io
        response = client.post(
            "/render-and-download",
            files={"dot_file": ("my_uploaded_graph.dot", io.BytesIO(b"digraph G { A -> B; }"), "text/plain")},
            data={
                "job_id": "cleanup_job"
            }
        )

        assert response.status_code == 500
        mock_save_upload_file.assert_called_once()
        mock_os_remove.assert_called_once_with(os.path.join(config.INPUT_DIR, "unique_filename.dot"))

