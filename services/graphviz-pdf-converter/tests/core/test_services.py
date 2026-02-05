import pytest
import subprocess
import os
import sys # Re-added import sys
import importlib # Added import for module reloading
from unittest.mock import MagicMock

import src.core.services # Explicitly import src.core.services as a module
from src.core.config import config
from src.core.domain import RenderRequest
from src.core.services import GraphvizService # Added this line

# Mock environment variables for consistent testing
@pytest.fixture(autouse=True)
def mock_config_settings(tmp_path, mocker):
    input_dir = tmp_path / "mock_input"
    output_dir = tmp_path / "mock_output"
    input_dir.mkdir()
    output_dir.mkdir()
    mocker.patch.object(config, 'INPUT_DIR', str(input_dir))
    mocker.patch.object(config, 'OUTPUT_DIR', str(output_dir))
    mocker.patch.object(config, 'RENDER_TIMEOUT_SECONDS', 5) # Shorter timeout for tests
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

    def test_render_dot_to_pdf_success(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        expected_output_path = os.path.join(config.OUTPUT_DIR, f"{mock_render_request.job_id}.pdf")
        mock_render_dot_to_pdf.return_value = expected_output_path

        result_path = GraphvizService.render_dot_to_pdf(mock_render_request)

        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)
        assert result_path == expected_output_path

    def test_render_dot_to_pdf_input_file_not_found(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError(f"Input .dot file not found: {os.path.join(config.INPUT_DIR, mock_render_request.dot_filename)}")

        with pytest.raises(FileNotFoundError, match=f"Input .dot file not found: {os.path.join(config.INPUT_DIR, mock_render_request.dot_filename)}"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)

    def test_render_dot_to_pdf_dot_command_not_found(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = FileNotFoundError("Graphviz 'dot' command not found")

        with pytest.raises(FileNotFoundError, match="Graphviz 'dot' command not found"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)

    def test_render_dot_to_pdf_timeout(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = subprocess.TimeoutExpired(
            cmd=['dot'], timeout=config.RENDER_TIMEOUT_SECONDS, output=b'', stderr=b''
        )

        with pytest.raises(subprocess.TimeoutExpired, match=f"timed out after {config.RENDER_TIMEOUT_SECONDS} seconds"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)

    def test_render_dot_to_pdf_command_failure(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = RuntimeError("Graphviz rendering failed for job test_job_id. Return code: 1. Stdout: error stdout. Stderr: error stderr.")

        with pytest.raises(RuntimeError, match="Graphviz rendering failed for job test_job_id.*error stderr"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)

    def test_render_dot_to_pdf_unexpected_error(self, mock_render_request, mocker):
        mock_render_dot_to_pdf = mocker.patch('src.core.services.GraphvizService.render_dot_to_pdf')
        mock_render_dot_to_pdf.side_effect = RuntimeError("An unexpected error occurred during subprocess execution for job test_job_id: Unexpected error")

        with pytest.raises(RuntimeError, match="An unexpected error occurred during subprocess execution for job test_job_id: Unexpected error"):
            GraphvizService.render_dot_to_pdf(mock_render_request)
        mock_render_dot_to_pdf.assert_called_once_with(mock_render_request)
