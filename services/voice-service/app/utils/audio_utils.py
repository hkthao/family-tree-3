import httpx # Changed from requests
import os
import tempfile
from typing import List, Tuple
from pydub import AudioSegment
from pydub.silence import split_on_silence
from loguru import logger

# Suppress pydub's warning about ffmpeg path if it's in PATH
AudioSegment.converter = "ffmpeg"
AudioSegment.ffmpeg = "ffmpeg"
AudioSegment.ffprobe = "ffprobe"


async def download_audio(url: str, output_path: str):
    """
    Downloads an audio file from a given URL asynchronously.
    """
    logger.info(f"Downloading audio from {url} to {output_path}")
    try:
        async with httpx.AsyncClient() as client: # Use httpx.AsyncClient
            response = await client.get(url)
            response.raise_for_status()
            with open(output_path, 'wb') as f:
                f.write(response.content)
        logger.info(f"Successfully downloaded audio from {url}")
    except httpx.RequestError as e: # Changed exception type
        logger.error(f"Error downloading audio from {url}: {e}")
        raise ValueError(f"Failed to download audio from {url}: {e}")


async def process_single_audio(input_path: str, output_path: str) -> float:
    """
    Processes a single audio file: converts to WAV, mono, 16kHz,
    normalizes volume, trims silence, and returns its duration.
    """
    logger.info(f"Processing audio file: {input_path}")
    try:
        audio = AudioSegment.from_file(input_path)

        # Convert to mono
        audio = audio.set_channels(1)
        logger.debug("Converted to mono.")

        # Set sample rate to 16kHz
        audio = audio.set_frame_rate(16000)
        logger.debug("Set sample rate to 16kHz.")

        # Normalize volume
        audio = audio.normalize()
        logger.debug("Normalized volume.")

        # Trim silence
        # Adjust parameters based on typical voice characteristics
        chunks = split_on_silence(
            audio,
            min_silence_len=500,  # milliseconds of silence
            silence_thresh=-40,    # dBFS below the average loudness
            keep_silence=100       # Keep 100ms of silence at chunk edges
        )
        if not chunks:
            logger.warning(f"No audio chunks found after trimming silence for {input_path}. Returning original audio.")
            processed_audio = audio
        else:
            processed_audio = AudioSegment.empty()
            for i, chunk in enumerate(chunks):
                processed_audio += chunk
            logger.debug(f"Trimmed silence. Original duration: {len(audio)}ms, Processed duration: {len(processed_audio)}ms.")

        # Export to WAV
        processed_audio.export(output_path, format="wav")
        logger.info(f"Successfully processed and exported {input_path} to {output_path}")

        return len(processed_audio) / 1000.0  # Duration in seconds
    except Exception as e:
        logger.error(f"Error processing audio file {input_path}: {e}")
        raise ValueError(f"Failed to process audio file {input_path}: {e}")


async def concatenate_audios(input_paths: List[str], output_path: str) -> float:
    """
    Concatenates multiple WAV audio files into a single WAV file, and returns its duration.
    """
    logger.info(f"Concatenating {len(input_paths)} audio files to {output_path}")
    combined_audio = AudioSegment.empty()
    for i, path in enumerate(input_paths):
        try:
            audio = AudioSegment.from_wav(path)
            combined_audio += audio
            logger.debug(f"Added {path} to combined audio ({i+1}/{len(input_paths)})")
        except Exception as e:
            logger.error(f"Error loading audio file for concatenation {path}: {e}")
            raise ValueError(f"Failed to load audio for concatenation {path}: {e}")

    try:
        combined_audio.export(output_path, format="wav")
        duration = len(combined_audio) / 1000.0
        logger.info(f"Successfully concatenated audios to {output_path}. Total duration: {duration:.2f}s")
        return duration
    except Exception as e:
        logger.error(f"Error exporting concatenated audio to {output_path}: {e}")
        raise ValueError(f"Failed to export concatenated audio to {output_path}: {e}")


async def get_audio_duration(file_path: str) -> float:
    """
    Returns the duration of an audio file in seconds.
    """
    logger.info(f"Getting duration for audio file: {file_path}")
    try:
        audio = AudioSegment.from_file(file_path)
        duration = len(audio) / 1000.0
        logger.info(f"Duration for {file_path}: {duration:.2f}s")
        return duration
    except Exception as e:
        logger.error(f"Error getting duration for audio file {file_path}: {e}")
        raise ValueError(f"Failed to get duration for audio file {file_path}: {e}")