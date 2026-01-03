import pytest
import os
import tempfile
from unittest.mock import AsyncMock
from pathlib import Path

from app.services.preprocess_service import PreprocessService
from app.config import settings


# Fixture for a temporary directory
@pytest.fixture
def temp_dir():
    with tempfile.TemporaryDirectory() as tmpdir:
        yield Path(tmpdir)


# Fixture to clear and reset STATIC_FILES_DIR before and after each test
@pytest.fixture(autouse=True)
def setup_static_files_dir():
    # Ensure STATIC_FILES_DIR exists
    settings.STATIC_FILES_DIR.mkdir(parents=True, exist_ok=True)
    # Clear directory before test
    for item in settings.STATIC_FILES_DIR.iterdir():
        if item.is_file():
            os.remove(item)
    yield
    # Clear directory after test
    for item in settings.STATIC_FILES_DIR.iterdir():
        if item.is_file():
            os.remove(item)


@pytest.mark.asyncio
async def test_process_audio_pipeline_success(temp_dir, mocker):
    service = PreprocessService()
    audio_urls = ["http://example.com/audio1.mp3", "http://example.com/audio2.m4a"]

    # Mock audio_utils functions
    mock_download_audio = mocker.patch("app.utils.audio_utils.download_audio", new_callable=AsyncMock)
    mock_process_single_audio = mocker.patch("app.utils.audio_utils.process_single_audio", new_callable=AsyncMock)
    mock_concatenate_audios = mocker.patch("app.utils.audio_utils.concatenate_audios", new_callable=AsyncMock)

    # Configure mock return values
    mock_process_single_audio.side_effect = [25.0, 30.0]  # durations > 20s

    def mock_concat_audios_side_effect(input_paths, output_path):
        # Create a dummy WAV file at the output_path
        with open(output_path, "wb") as f:
            f.write(b"dummy wav data")
        return 55.0  # return the duration

    mock_concatenate_audios.side_effect = mock_concat_audios_side_effect

    processed_url, duration = await service.process_audio_pipeline(audio_urls)

    # Assertions
    mock_download_audio.assert_called()
    assert mock_download_audio.call_count == 2
    mock_process_single_audio.assert_called()
    assert mock_process_single_audio.call_count == 2
    mock_concatenate_audios.assert_called_once()

    # Check if a file was moved to STATIC_FILES_DIR
    assert len(list(settings.STATIC_FILES_DIR.iterdir())) == 1
    final_file = list(settings.STATIC_FILES_DIR.iterdir())[0]

    # Check the returned URL format
    assert processed_url.startswith(settings.VOICE_SERVICE_BASE_URL + "/static/")
    assert final_file.name in processed_url
    assert duration == 55.0
    assert final_file.exists()


@pytest.mark.asyncio
async def test_process_audio_pipeline_empty_urls():
    service = PreprocessService()
    with pytest.raises(ValueError, match="No audio URLs provided"):
        await service.process_audio_pipeline([])


@pytest.mark.asyncio
async def test_process_audio_pipeline_short_audio(temp_dir, mocker):
    service = PreprocessService()
    audio_urls = ["http://example.com/short_audio.mp3"]

    mocker.patch("app.utils.audio_utils.download_audio", new_callable=AsyncMock)
    mock_process_single_audio = mocker.patch("app.utils.audio_utils.process_single_audio", new_callable=AsyncMock)
    mocker.patch("app.utils.audio_utils.concatenate_audios", new_callable=AsyncMock)

    mock_process_single_audio.return_value = 15.0  # duration < 20s

    with pytest.raises(ValueError, match="is too short"):
        await service.process_audio_pipeline(audio_urls)

    # Ensure no files were left in static_files_dir if processing failed early
    assert len(list(settings.STATIC_FILES_DIR.iterdir())) == 0


@pytest.mark.asyncio
async def test_process_audio_pipeline_download_failure(temp_dir, mocker):
    service = PreprocessService()
    audio_urls = ["http://example.com/bad_audio.mp3"]

    mock_download_audio = mocker.patch("app.utils.audio_utils.download_audio", new_callable=AsyncMock)
    mock_download_audio.side_effect = ValueError("Download failed")

    with pytest.raises(ValueError, match="Download failed"):
        await service.process_audio_pipeline(audio_urls)
    assert len(list(settings.STATIC_FILES_DIR.iterdir())) == 0


@pytest.mark.asyncio
async def test_process_audio_pipeline_processing_failure(temp_dir, mocker):
    service = PreprocessService()
    audio_urls = ["http://example.com/problem_audio.mp3"]

    mocker.patch("app.utils.audio_utils.download_audio", new_callable=AsyncMock)
    mock_process_single_audio = mocker.patch("app.utils.audio_utils.process_single_audio", new_callable=AsyncMock)
    mock_process_single_audio.side_effect = ValueError("Processing error")

    with pytest.raises(ValueError, match="Processing error"):
        await service.process_audio_pipeline(audio_urls)
    assert len(list(settings.STATIC_FILES_DIR.iterdir())) == 0


@pytest.mark.asyncio
async def test_process_audio_pipeline_concatenation_failure(temp_dir, mocker):
    service = PreprocessService()
    audio_urls = ["http://example.com/audio1.mp3", "http://example.com/audio2.mp3"]

    mocker.patch("app.utils.audio_utils.download_audio", new_callable=AsyncMock)
    mocker.patch("app.utils.audio_utils.process_single_audio", AsyncMock(return_value=25.0))
    mock_concatenate_audios = mocker.patch("app.utils.audio_utils.concatenate_audios", new_callable=AsyncMock)
    mock_concatenate_audios.side_effect = ValueError("Concatenation error")

    with pytest.raises(ValueError, match="Concatenation error"):
        await service.process_audio_pipeline(audio_urls)
    assert len(list(settings.STATIC_FILES_DIR.iterdir())) == 0