import pytest
import subprocess
import os
import sys
from unittest.mock import MagicMock, patch

# Adjust path to import modules correctly
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..')))
from src.core.config import config
from src.core.domain import RenderRequest
from src.core.services import GraphvizService
sys.path.pop(0)

# Mock environment variables for consistent testing
@pytest.fixture(autouse=True)
def mock_config_settings():
    with (
        patch.object(config, 'INPUT_DIR', '/mock/input'),
        patch.object(config, 'OUTPUT_DIR', '/mock/output'),
        patch.object(config, 'RENDER_TIMEOUT_SECONDS', 5) # Shorter timeout for tests
    ):
        yield

@pytest.fixture
def mock_render_request():
    return RenderRequest(
        job_id="test_job_id",
        dot_filename="test.dot",
        page_size="A0",
        direction="LR"
    )

class TestGraphvizService:

    @patch('os.path.exists')
    @patch('subprocess.run')
    def test_render_dot_to_pdf_success(self, mock_subprocess_run, mock_os_path_exists, mock_render_request):
        mock_os_path_exists.return_value = True
        mock_subprocess_run.return_value = MagicMock(returncode=0, stdout="dot output", stderr="")

        result_path = GraphvizService.render_dot_to_pdf(mock_render_request)

        expected_input_path = os.path.join(config.INPUT_DIR, mock_render_request.dot_filename)
        expected_output_path = os.path.join(config.OUTPUT_DIR, f"{mock_render_request.job_id}.pdf")

        mock_os_path_exists.assert_called_once_with(expected_input_path)
        mock_subprocess_run.assert_called_once_with(
            [
                'dot',
                '-Tpdf',
                f'-Grankdir={mock_render_request.direction}',
                f'-s{mock_render_request.page_size}',
                expected_input_path,
                '-o',
                expected_output_path
            ],
            capture_output=True,
            text=True,
            timeout=config.RENDER_TIMEOUT_SECONDS,
            check=False
        )
        assert result_path == expected_output_path

    @patch('os.path.exists')
    def test_render_dot_to_pdf_input_file_not_found(self, mock_os_path_exists, mock_render_request):
        mock_os_path_exists.return_value = False

        with pytest.raises(FileNotFoundError, match=f"Input .dot file not found: {os.path.join(config.INPUT_DIR, mock_render_request.dot_filename)}"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_os_path_exists.assert_called_once()

    @patch('os.path.exists', return_value=True)
    @patch('subprocess.run', side_effect=FileNotFoundError("dot not found"))
    def test_render_dot_to_pdf_dot_command_not_found(self, mock_subprocess_run, mock_os_path_exists, mock_render_request):
        with pytest.raises(FileNotFoundError, match="Graphviz 'dot' command not found. Is Graphviz installed and in PATH?"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_subprocess_run.assert_called_once()

    @patch('os.path.exists', return_value=True)
    @patch('subprocess.run')
    def test_render_dot_to_pdf_timeout(self, mock_subprocess_run, mock_os_path_exists, mock_render_request):
        mock_subprocess_run.side_effect = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b''
        )

        with pytest.raises(subprocess.TimeoutExpired, match=f"timed out after {config.RENDER_TIMEOUT_SECONDS} seconds"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_subprocess_run.assert_called_once()

    @patch('os.path.exists', return_value=True)
    @patch('subprocess.run')
    def test_render_dot_to_pdf_command_failure(self, mock_subprocess_run, mock_os_path_exists, mock_render_request):
        mock_subprocess_run.return_value = MagicMock(
            returncode=1, stdout="error stdout", stderr="error stderr"
        )

        with pytest.raises(RuntimeError, match="Graphviz rendering failed for job test_job_id.*error stderr"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_subprocess_run.assert_called_once()

    @patch('os.path.exists', return_value=True)
    @patch('subprocess.run', side_effect=Exception("Unexpected error"))
    def test_render_dot_to_pdf_unexpected_error(self, mock_subprocess_run, mock_os_path_exists, mock_render_request):
        with pytest.raises(RuntimeError, match="An unexpected error occurred during subprocess execution for job test_job_id: Unexpected error"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_subprocess_run.assert_called_once()
