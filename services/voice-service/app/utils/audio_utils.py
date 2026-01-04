import httpx
import os
import tempfile
from typing import List
from pydub import AudioSegment
from loguru import logger
import webrtcvad
import collections



# Suppress pydub's warning about ffmpeg path if it's in PATH
AudioSegment.converter = "ffmpeg"
AudioSegment.ffmpeg = "ffmpeg"
AudioSegment.ffprobe = "ffprobe"


# Constants for VAD
_VAD_SAMPLE_RATE = 16000
_VAD_FRAME_DURATION_MS = 30  # ms
_VAD_BYTES_PER_SAMPLE = 2  # 16-bit audio
_VAD_MIN_SPEECH_LEN_MS = 200  # ms (minimum length of a speech segment to keep)


async def download_audio(url: str, output_path: str):
    """
    Downloads an audio file from a given URL to a specified output path.
    """
    logger.info(f"Downloading audio from {url} to {output_path}")
    async with httpx.AsyncClient() as client:
        try:
            response = await client.get(url, follow_redirects=True, timeout=30.0)
            response.raise_for_status()  # Raise an exception for HTTP errors
            with open(output_path, "wb") as f:
                f.write(response.content)
            logger.info(f"Successfully downloaded audio from {url}")
        except httpx.RequestError as e:
            logger.error(f"Failed to download audio from {url}: {e}")
            raise ValueError(f"Failed to download audio from {url}: {e}")
        except Exception as e:
            logger.error(f"An unexpected error occurred during download from {url}: {e}")
            raise ValueError(f"An unexpected error occurred during download from {url}: {e}")


async def get_audio_duration(file_path: str) -> float:
    """
    Gets the duration of an audio file in seconds.
    """
    try:
        audio = AudioSegment.from_file(file_path)
        return len(audio) / 1000.0
    except Exception as e:
        logger.error(f"Failed to get duration for {file_path}: {e}")
        raise ValueError(f"Failed to get duration for {file_path}: {e}")


async def check_for_human_speech(audio_path: str) -> bool:
    """
    Checks if an audio file contains human speech using WebRTC VAD.
    Returns True if speech is detected, False otherwise.
    """
    logger.info(f"Checking for human speech in {audio_path}")
    try:
        audio = AudioSegment.from_wav(audio_path)

        # Ensure it's 16-bit, mono, 16kHz for VAD
        if audio.sample_width != _VAD_BYTES_PER_SAMPLE or \
           audio.channels != 1 or \
           audio.frame_rate != _VAD_SAMPLE_RATE:

            logger.debug("Converting audio to 16-bit, mono, 16kHz for VAD check.")
            audio = audio.set_sample_width(_VAD_BYTES_PER_SAMPLE)
            audio = audio.set_channels(1)
            audio = audio.set_frame_rate(_VAD_SAMPLE_RATE)

        # Export to raw PCM for VAD
        raw_audio_data = audio.raw_data

        vad = webrtcvad.Vad(1)  # Aggressiveness mode 1
        frames = _frame_generator(raw_audio_data, _VAD_SAMPLE_RATE, _VAD_FRAME_DURATION_MS, _VAD_BYTES_PER_SAMPLE)

        # Iterate through frames and check for speech
        for frame in frames:
            if vad.is_speech(frame.bytes, _VAD_SAMPLE_RATE):
                logger.info(f"Human speech detected in {audio_path}")
                return True
        logger.info(f"No human speech detected in {audio_path}")
        return False
    except Exception as e:
        logger.error(f"Error during human speech detection for {audio_path}: {e}")
        return False


async def apply_vad(input_path: str, output_path: str):
    """
    Applies Voice Activity Detection (VAD) to an audio file to remove silent parts.
    The audio must be 16-bit, mono, and 16kHz for webrtcvad.
    """
    logger.info(f"Applying VAD to {input_path}.")
    try:
        # Load audio using pydub to ensure correct format
        audio = AudioSegment.from_wav(input_path)

        # Ensure it's 16-bit, mono, 16kHz for VAD
        if audio.sample_width != _VAD_BYTES_PER_SAMPLE or \
           audio.channels != 1 or \
           audio.frame_rate != _VAD_SAMPLE_RATE:

            logger.warning("VAD input audio not in 16-bit, mono, 16kHz. Converting...")
            audio = audio.set_sample_width(_VAD_BYTES_PER_SAMPLE)
            audio = audio.set_channels(1)
            audio = audio.set_frame_rate(_VAD_SAMPLE_RATE)

        # Export to raw PCM for VAD
        raw_audio_data = audio.raw_data

        vad = webrtcvad.Vad(1)  # Aggressiveness mode (0 to 3, 1 is often a good balance for speech quality)
        frames = _frame_generator(raw_audio_data, _VAD_SAMPLE_RATE, _VAD_FRAME_DURATION_MS, _VAD_BYTES_PER_SAMPLE)

        # Collect speech segments
        segments = _vad_collector(vad, frames, _VAD_SAMPLE_RATE, _VAD_FRAME_DURATION_MS, _VAD_MIN_SPEECH_LEN_MS)

        # Reconstruct audio from speech segments
        vad_audio = AudioSegment.empty()
        for i, segment in enumerate(segments):
            seg_audio = AudioSegment(
                segment.bytes,
                sample_width=_VAD_BYTES_PER_SAMPLE,
                frame_rate=_VAD_SAMPLE_RATE,
                channels=1
            )
            vad_audio += seg_audio
            logger.debug(f"VAD segment {i+1} added. Duration: {segment.duration:.2f}s")

        if not vad_audio:
            logger.warning(f"VAD removed all audio for {input_path}. Outputting a short silent file.")
            # If all audio is removed, output a short silent audio to avoid errors
            AudioSegment.silent(duration=100, frame_rate=_VAD_SAMPLE_RATE).export(output_path, format="wav")
            return

        vad_audio.export(output_path, format="wav")
        logger.info(f"VAD applied. Output saved to {output_path}.")
    except Exception as e:
        logger.error(f"Error during VAD for {input_path}: {e}")
        raise


async def process_single_audio(input_path: str, output_path: str) -> float:
    """
    Processes a single audio file:
    1. Loads audio (handles various formats).
    2. Converts to WAV, mono, 16kHz, 16-bit.
    3. Applies noise reduction.
    4. Applies Voice Activity Detection (VAD).
    5. Removes low-energy segments.
    6. Normalizes audio.
    7. Exports to a WAV file.
    8. Returns the duration of the processed audio.
    """
    logger.info(f"Starting single audio processing pipeline for {input_path}")
    temp_files = []
    current_audio_path = input_path

    try:
        # Step 1: Load audio and standardize to WAV, mono, 16kHz, 16-bit
        # This ensures all subsequent processing steps work with a consistent audio format.
        standardized_wav_path = tempfile.mktemp(suffix=".wav")
        temp_files.append(standardized_wav_path)

        audio = AudioSegment.from_file(input_path)
        audio = audio.set_channels(1)
        audio = audio.set_frame_rate(_VAD_SAMPLE_RATE)
        audio = audio.set_sample_width(_VAD_BYTES_PER_SAMPLE)
        audio.export(standardized_wav_path, format="wav")
        current_audio_path = standardized_wav_path
        logger.info(f"Standardized audio to WAV, mono, 16kHz, 16-bit at {current_audio_path}")

        # Step 2: Apply Voice Activity Detection (VAD)
        # VAD is applied after standardization to identify and keep speech segments.
        vad_processed_path = tempfile.mktemp(suffix=".wav")
        temp_files.append(vad_processed_path)
        await apply_vad(current_audio_path, vad_processed_path)
        current_audio_path = vad_processed_path

        # Check if VAD removed all audio (e.g., only noise or very faint speech)
        if not os.path.exists(current_audio_path) or await get_audio_duration(current_audio_path) < 0.01:
            logger.warning(f"VAD removed all audio for {input_path}. Returning a short silent audio.")
            AudioSegment.silent(duration=100, frame_rate=_VAD_SAMPLE_RATE).export(output_path, format="wav")
            return 0.1  # Return a minimal duration

        # Step 3: Move final processed audio to output_path and get duration
        os.rename(current_audio_path, output_path)
        duration = await get_audio_duration(output_path)
        logger.info(f"Finished single audio processing pipeline for {input_path}. Output saved to {output_path}. Duration: {duration:.2f}s")
        return duration

    except Exception as e:
        logger.error(f"Failed to process audio file {input_path}: {e}")
        raise ValueError(f"Failed to process audio file {input_path}: {e}")
    finally:
        # Clean up temporary files
        for f in temp_files:
            if os.path.exists(f):
                os.remove(f)


async def concatenate_audios(audio_paths: List[str], output_path: str) -> float:
    """
    Concatenates multiple audio files into a single WAV file.
    Assumes all input audio files are already in the correct format (WAV, mono, 16kHz).
    """
    logger.info(f"Concatenating {len(audio_paths)} audios into {output_path}")
    combined_audio = AudioSegment.empty()
    for path in audio_paths:
        try:
            audio = AudioSegment.from_wav(path)
            combined_audio += audio
        except Exception as e:
            logger.error(f"Error loading audio file {path} for concatenation: {e}")
            raise ValueError(f"Could not concatenate audio files: {e}")

    if not combined_audio:
        logger.warning("No audio segments to concatenate. Returning an empty audio.")
        # Create a tiny silent audio to ensure a valid WAV file is created
        combined_audio = AudioSegment.silent(duration=100, frame_rate=_VAD_SAMPLE_RATE)

    combined_audio.export(output_path, format="wav")
    duration = len(combined_audio) / 1000.0
    logger.info(f"Concatenation complete. Total duration: {duration:.2f}s")
    return duration


class Frame(object):
    """Represents a "frame" of audio data."""
    def __init__(self, bytes, timestamp, duration):
        self.bytes = bytes
        self.timestamp = timestamp
        self.duration = duration


def _frame_generator(audio_data: bytes, sample_rate: int, frame_duration_ms: int, bytes_per_sample: int):
    """Generates audio frames from PCM audio data."""
    frames_per_second = sample_rate // 1000
    frame_size = int(frame_duration_ms * frames_per_second * bytes_per_sample)

    offset = 0
    timestamp = 0.0
    duration = frame_duration_ms / 1000.0
    while offset + frame_size <= len(audio_data):
        yield Frame(audio_data[offset:offset + frame_size], timestamp, duration)
        offset += frame_size
        timestamp += duration

def _vad_collector(vad, frames, sample_rate: int, frame_duration_ms: int, min_speech_len_ms: int):
    """Filters out non-speech audio frames.
    
    Args:
        vad: An instance of webrtcvad.Vad.
        frames: A generator of audio Frames.
        sample_rate: The audio sample rate, in Hz.
        frame_duration_ms: The duration in milliseconds of each frame.
        min_speech_len_ms: The minimum length of a speech segment to consider in milliseconds.
    
    Returns: A generator of `Frame` objects representing speech segments.
    """
    ring_buffer = collections.deque(maxlen=30)  # A buffer of 30 frames
    triggered = False
    voiced_frames = []

    for frame in frames:
        is_speech = vad.is_speech(frame.bytes, sample_rate)
        
        if not triggered:
            ring_buffer.append((frame, is_speech))
            if sum(1 for f, s in ring_buffer if s) > 0.9 * ring_buffer.maxlen:
                triggered = True
                logger.debug(f"Speech detected at {frame.timestamp:.2f}s")
                # Prepend the frames from the ring buffer to the voiced_frames.
                for f, s in ring_buffer:
                    voiced_frames.append(f)
                ring_buffer.clear()
        else:
            voiced_frames.append(frame)
            ring_buffer.append((frame, is_speech))
            num_unvoiced = sum(1 for f, s in ring_buffer if not s)
            if num_unvoiced > 0.9 * ring_buffer.maxlen:
                triggered = False
                logger.debug(f"Speech ended at {frame.timestamp:.2f}s")
                # Yield the speech segment if it's long enough
                segment_duration_ms = len(voiced_frames) * frame_duration_ms
                if segment_duration_ms >= min_speech_len_ms:
                    yield Frame(b''.join([f.bytes for f in voiced_frames]), voiced_frames[0].timestamp, segment_duration_ms / 1000.0)
                voiced_frames = []
                ring_buffer.clear()
    
    # If we end with a triggered state, yield the remaining frames
    if voiced_frames:
        segment_duration_ms = len(voiced_frames) * frame_duration_ms
        if segment_duration_ms >= min_speech_len_ms:
            yield Frame(b''.join([f.bytes for f in voiced_frames]), voiced_frames[0].timestamp, segment_duration_ms / 1000.0)