import pytest
import os
import tempfile
from unittest.mock import AsyncMock
from pydub import AudioSegment
import numpy as np
import httpx  # Added import for httpx

# Import the module itself, not just functions, to allow patching its attributes
import app.utils.audio_utils as audio_utils_module


# Helper to create an audio segment with a specified dBFS
def create_audio_with_dbfs(duration_ms: int, frame_rate: int, target_dbfs: int) -> AudioSegment:
    # Generate a simple sine wave using numpy
    samples = np.array([int(10000 * np.sin(2 * np.pi * 440 * i / frame_rate)) for i in range(0, frame_rate * duration_ms // 1000)])  # Reduced amplitude to avoid clipping
    audio = AudioSegment(
        samples.tobytes(),
        frame_rate=frame_rate,
        sample_width=2,  # 16-bit
        channels=1
    )
    # Adjust its gain to reach the target dBFS
    change_in_dbfs = target_dbfs - audio.dBFS
    audio = audio.apply_gain(change_in_dbfs)
    return audio


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
async def test_download_audio_success(temp_dir, mocker):
    url = "http://example.com/audio.wav"
    output_path = os.path.join(temp_dir, "downloaded.wav")
    mock_content = b"dummy audio data"

    mock_client_instance = AsyncMock()
    mock_client_instance.__aenter__.return_value = mock_client_instance
    mock_response = mocker.MagicMock()
    mock_response.raise_for_status = mocker.MagicMock()
    mock_response.content = mock_content
    mock_client_instance.get.return_value = mock_response

    mocker.patch("httpx.AsyncClient", return_value=mock_client_instance)

    await audio_utils_module.download_audio(url, output_path)

    assert os.path.exists(output_path)
    with open(output_path, "rb") as f:
        assert f.read() == mock_content
    # Corrected assertion to match actual call arguments
    mock_client_instance.get.assert_called_once_with(
        url, follow_redirects=True, timeout=30.0  # Added these arguments
    )


@pytest.mark.asyncio
async def test_download_audio_failure(temp_dir, mocker):
    url = "http://example.com/nonexistent.wav"
    output_path = os.path.join(temp_dir, "downloaded.wav")

    mock_client_instance = AsyncMock()
    mock_client_instance.__aenter__.return_value = mock_client_instance
    mock_client_instance.get.side_effect = httpx.RequestError("Download failed", request=mocker.MagicMock())

    mocker.patch("httpx.AsyncClient", return_value=mock_client_instance)

    with pytest.raises(ValueError, match="Failed to download audio"):
        await audio_utils_module.download_audio(url, output_path)
    assert not os.path.exists(output_path)


@pytest.mark.asyncio
async def test_process_single_audio_wav(temp_dir, dummy_mp3_file, mocker):
    output_path = os.path.join(temp_dir, "processed.wav")

    # Mock internal audio processing steps by patching them directly on the module
    mock_apply_vad = mocker.patch("app.utils.audio_utils.apply_vad", new_callable=AsyncMock)
    mocker.patch("app.utils.audio_utils.get_audio_duration", new_callable=AsyncMock, return_value=1.0)  # Mock duration for checks

    # Simulate output files for each step
    def create_dummy_wav(input_file, output_file, *args, **kwargs):
        AudioSegment.silent(duration=500, frame_rate=16000).export(output_file, format="wav")

    mock_apply_vad.side_effect = create_dummy_wav

    duration = await audio_utils_module.process_single_audio(dummy_mp3_file, output_path)

    # Assert that VAD function was called
    mock_apply_vad.assert_called_once()

    # Assertions for the final output
    assert os.path.exists(output_path)
    assert isinstance(duration, float)
    assert duration == 1.0  # Expected duration from mock_get_audio_duration

    processed_audio = AudioSegment.from_wav(output_path)
    assert processed_audio.channels == 1
    assert processed_audio.frame_rate == 16000


@pytest.mark.asyncio
async def test_process_single_audio_error(temp_dir):
    invalid_file = os.path.join(temp_dir, "invalid.txt")
    with open(invalid_file, "w") as f:
        f.write("This is not audio data")
    output_path = os.path.join(temp_dir, "processed.wav")

    with pytest.raises(ValueError, match="Failed to process audio file"):
        await audio_utils_module.process_single_audio(invalid_file, output_path)
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

    total_duration = await audio_utils_module.concatenate_audios(input_paths, output_path)

    assert os.path.exists(output_path)
    assert isinstance(total_duration, float)

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
    total_duration = await audio_utils_module.concatenate_audios([], output_path)
    assert os.path.exists(output_path)
    assert isinstance(total_duration, float)
    assert total_duration > 0  # Should return a minimal duration from silent audio





@pytest.mark.asyncio
async def test_get_audio_duration_success(dummy_wav_file):
    duration = await audio_utils_module.get_audio_duration(dummy_wav_file)
    assert isinstance(duration, float)
    assert abs(duration - 1.0) < 0.01  # Dummy file is 1 second


@pytest.mark.asyncio
async def test_get_audio_duration_failure(temp_dir):
    non_audio_file = os.path.join(temp_dir, "text.txt")
    with open(non_audio_file, "w") as f:
        f.write("This is not audio")
    with pytest.raises(ValueError, match="Failed to get duration"):
        await audio_utils_module.get_audio_duration(non_audio_file)
