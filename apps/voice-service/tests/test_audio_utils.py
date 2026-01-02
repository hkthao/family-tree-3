import pytest
import os
import tempfile
from unittest.mock import AsyncMock, patch, MagicMock
from pydub import AudioSegment
import numpy as np
import httpx # Added import for httpx

from app.utils.audio_utils import (
    download_audio,
    process_single_audio,
    concatenate_audios,
    get_audio_duration,
)


@pytest.fixture
def temp_dir():
    with tempfile.TemporaryDirectory() as tmpdir:
        yield tmpdir


@pytest.fixture
def dummy_wav_file(temp_dir):
    # Create a simple 1-second mono 16kHz WAV file
    filepath = os.path.join(temp_dir, "dummy.wav")
    audio = AudioSegment.silent(duration=1000, frame_rate=16000).set_channels(1)
    audio.export(filepath, format="wav")
    return filepath


@pytest.fixture
def dummy_mp3_file(temp_dir):
    # Create a simple 1-second mono MP3 file
    filepath = os.path.join(temp_dir, "dummy.mp3")
    audio = AudioSegment.silent(duration=1000, frame_rate=16000).set_channels(1)
    audio.export(filepath, format="mp3")
    return filepath


@pytest.mark.asyncio
async def test_download_audio_success(temp_dir):
    url = "http://example.com/audio.wav"
    output_path = os.path.join(temp_dir, "downloaded.wav")
    mock_content = b"dummy audio data"

    with patch("httpx.AsyncClient") as MockAsyncClient:
        mock_response = MagicMock()
        mock_response.content = mock_content
        mock_response.raise_for_status = MagicMock()
        MockAsyncClient.return_value.__aenter__.return_value.get.return_value = (
            mock_response
        )

        await download_audio(url, output_path)

        assert os.path.exists(output_path)
        with open(output_path, "rb") as f:
            assert f.read() == mock_content
        MockAsyncClient.return_value.__aenter__.return_value.get.assert_called_once_with(
            url
        )


@pytest.mark.asyncio
async def test_download_audio_failure(temp_dir):
    url = "http://example.com/nonexistent.wav"
    output_path = os.path.join(temp_dir, "downloaded.wav")

    with patch("httpx.AsyncClient") as MockAsyncClient:
        MockAsyncClient.return_value.__aenter__.return_value.get.side_effect = (
            httpx.RequestError("Download failed", request=MagicMock())
        )

        with pytest.raises(ValueError, match="Failed to download audio"):
            await download_audio(url, output_path)
        assert not os.path.exists(output_path)


@pytest.mark.asyncio
async def test_process_single_audio_wav(temp_dir, dummy_mp3_file):
    output_path = os.path.join(temp_dir, "processed.wav")
    duration = await process_single_audio(dummy_mp3_file, output_path)

    assert os.path.exists(output_path)
    assert isinstance(duration, float)
    assert duration > 0.0

    processed_audio = AudioSegment.from_wav(output_path)
    assert processed_audio.channels == 1
    assert processed_audio.frame_rate == 16000
    # The exact duration might change slightly after silence trimming
    assert (
        abs(duration - (len(processed_audio) / 1000.0)) < 0.01
    )  # Check reported duration vs actual


@pytest.mark.asyncio
async def test_process_single_audio_error(temp_dir):
    invalid_file = os.path.join(temp_dir, "invalid.txt")
    with open(invalid_file, "w") as f:
        f.write("This is not audio data")
    output_path = os.path.join(temp_dir, "processed.wav")

    with pytest.raises(ValueError, match="Failed to process audio file"):
        await process_single_audio(invalid_file, output_path)
    assert not os.path.exists(output_path)


@pytest.mark.asyncio
async def test_concatenate_audios_success(temp_dir, dummy_wav_file):
    file1 = dummy_wav_file
    file2 = os.path.join(temp_dir, "dummy2.wav")
    file3 = os.path.join(temp_dir, "dummy3.wav")

    # Create two more dummy wav files
    AudioSegment.silent(duration=500, frame_rate=16000).set_channels(1).export(
        file2, format="wav"
    )
    AudioSegment.silent(duration=1500, frame_rate=16000).set_channels(1).export(
        file3, format="wav"
    )

    input_paths = [file1, file2, file3]
    output_path = os.path.join(temp_dir, "concatenated.wav")

    total_duration = await concatenate_audios(input_paths, output_path)

    assert os.path.exists(output_path)
    assert isinstance(total_duration, float)

    combined_audio = AudioSegment.from_wav(output_path)
    # Total duration should be sum of individual durations
    expected_duration = (
        len(AudioSegment.from_wav(file1))
        + len(AudioSegment.from_wav(file2))
        + len(AudioSegment.from_wav(file3))
    ) / 1000.0
    assert abs(total_duration - expected_duration) < 0.01


@pytest.mark.asyncio
async def test_concatenate_audios_empty_list(temp_dir):
    output_path = os.path.join(temp_dir, "concatenated.wav")
    combined_audio = AudioSegment.empty()
    combined_audio.export(output_path, format="wav")
    total_duration = await concatenate_audios([], output_path)
    assert os.path.exists(output_path)
    assert isinstance(total_duration, float)


@pytest.mark.asyncio
async def test_get_audio_duration_success(dummy_wav_file):
    duration = await get_audio_duration(dummy_wav_file)
    assert isinstance(duration, float)
    assert abs(duration - 1.0) < 0.01  # Dummy file is 1 second


@pytest.mark.asyncio
async def test_get_audio_duration_failure(temp_dir):
    non_audio_file = os.path.join(temp_dir, "text.txt")
    with open(non_audio_file, "w") as f:
        f.write("This is not audio")
    with pytest.raises(ValueError, match="Failed to get duration"):
        await get_audio_duration(non_audio_file)
