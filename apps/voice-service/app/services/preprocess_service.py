import os
import tempfile
from typing import List, Tuple
from loguru import logger
from app.utils import audio_utils
from app.services.storage_service import S3StorageService, get_s3_storage_service
from pathlib import Path


class PreprocessService:
    def __init__(self):
        self.storage_service = get_s3_storage_service()
        logger.info("PreprocessService initialized.")

    async def process_audio_pipeline(self, audio_urls: List[str]) -> Tuple[str, float]:
        """
        Orchestrates the audio preprocessing pipeline:
        1. Downloads each audio from URL.
        2. Processes each audio (WAV, mono, 16kHz, normalize, trim silence).
        3. Concatenates all processed audios into a single WAV file.
        4. Uploads the final WAV file to object storage.
        5. Returns the URL of the processed audio and its total duration.
        """
        if not audio_urls:
            raise ValueError("No audio URLs provided for preprocessing.")

        temp_dir_path = None
        downloaded_paths = []
        processed_paths = []
        try:
            # Create a temporary directory
            temp_dir_path = Path(tempfile.mkdtemp())
            logger.info(f"Created temporary directory: {temp_dir_path}")

            # 1. & 2. Download and process each audio
            for i, url in enumerate(audio_urls):
                downloaded_file = temp_dir_path / f"downloaded_{i}.tmp"
                processed_file = temp_dir_path / f"processed_{i}.wav"

                await audio_utils.download_audio(url, str(downloaded_file))
                downloaded_paths.append(downloaded_file)

                duration = await audio_utils.process_single_audio(str(downloaded_file), str(processed_file))
                if duration < 20.0: # Check duration after processing
                    raise ValueError(f"Processed audio from {url} is too short ({duration:.2f}s). Minimum 20s required.")
                processed_paths.append(processed_file)

            # 3. Concatenate all processed audios
            merged_audio_file = temp_dir_path / "merged_output.wav"
            total_duration = await audio_utils.concatenate_audios(
                [str(p) for p in processed_paths], str(merged_audio_file)
            )

            # 4. Upload the final WAV file to object storage
            object_name = f"processed_voices/{Path(merged_audio_file).name}"
            processed_audio_url = await self.storage_service.upload_file(
                str(merged_audio_file), object_name, content_type="audio/wav"
            )

            logger.info(f"Audio preprocessing pipeline completed. Final URL: {processed_audio_url}, Duration: {total_duration:.2f}s")
            return processed_audio_url, total_duration

        except Exception as e:
            logger.error(f"Error during audio preprocessing pipeline: {e}")
            raise
        finally:
            # 5. Clean up temporary files and directory
            if temp_dir_path and temp_dir_path.exists():
                for path in downloaded_paths + processed_paths + [merged_audio_file]:
                    if path.exists():
                        os.remove(path)
                os.rmdir(temp_dir_path)
                logger.info(f"Cleaned up temporary directory: {temp_dir_path}")


_preprocess_service_instance: PreprocessService = None

def get_preprocess_service() -> PreprocessService:
    global _preprocess_service_instance
    if _preprocess_service_instance is None:
        _preprocess_service_instance = PreprocessService()
    return _preprocess_service_instance
