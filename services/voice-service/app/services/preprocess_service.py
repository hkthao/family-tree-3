import tempfile
import shutil
from dataclasses import dataclass, field
from pydub import AudioSegment
from typing import List, Tuple
from loguru import logger
from app.utils import audio_utils
from pathlib import Path
import uuid
import os
from app.config import settings


@dataclass
class AudioQualityReport:
    overall_quality: str  # "pass", "warn", "reject"
    quality_score: float
    messages: List[str] = field(default_factory=list)


class PreprocessService:
    def __init__(self):
        logger.info("PreprocessService initialized.")

    async def _perform_audio_quality_checks(self, audio_path: str, duration: float) -> AudioQualityReport:
        messages = []
        overall_quality = "pass"
        quality_score = 1.0  # Start with a perfect score

        # Load audio for quality checks
        try:
            audio = AudioSegment.from_wav(audio_path)
        except Exception as e:
            logger.error(f"Could not load audio for quality checks: {e}")
            return AudioQualityReport(overall_quality="reject", quality_score=0.0, messages=[f"Không thể tải tệp âm thanh: {e}"])

        # 1. Check for human speech using VAD
        has_speech = await audio_utils.check_for_human_speech(audio_path)
        if not has_speech:
            overall_quality = "reject"
            quality_score *= 0.1  # Significant penalty for no speech
            messages.append("REJECT: Không phát hiện thấy giọng nói của con người. Có thể là im lặng hoặc chỉ có tiếng ồn.")
        else:
            messages.append("PASS: Phát hiện thấy giọng nói của con người.")

        # 2. Basic loudness check (dBFS - average loudness)
        # Assuming a reasonable range for speech, e.g., between -25 dBFS and -5 dBFS
        # This is a heuristic and might need adjustment based on expected input.
        average_loudness_dbfs = audio.dBFS
        if average_loudness_dbfs < -35:
            overall_quality = "warn" if overall_quality == "pass" else overall_quality
            quality_score *= 0.7  # Penalty for being too quiet
            messages.append(f"WARN: Âm lượng trung bình quá thấp ({average_loudness_dbfs:.2f} dBFS).")
        elif average_loudness_dbfs > -5:
            overall_quality = "warn" if overall_quality == "pass" else overall_quality
            quality_score *= 0.8  # Penalty for being too loud
            messages.append(f"WARN: Âm lượng trung bình quá cao ({average_loudness_dbfs:.2f} dBFS).")
        else:
            messages.append(f"PASS: Âm lượng trung bình chấp nhận được ({average_loudness_dbfs:.2f} dBFS).")

        # 3. Clipping check (max_dBFS close to 0 dBFS)
        # Max_dBFS should ideally be below 0 dBFS. Values very close to 0 might indicate clipping.
        max_loudness_dbfs = audio.max_dBFS
        if max_loudness_dbfs >= -1.0:  # Threshold for potential clipping
            overall_quality = "warn" if overall_quality == "pass" else overall_quality
            quality_score *= 0.7  # Penalty for potential clipping
            messages.append(f"WARN: Có khả năng bị cắt tiếng (max_dBFS: {max_loudness_dbfs:.2f} dBFS).")
        else:
            messages.append(f"PASS: Không phát hiện thấy cắt tiếng đáng kể (max_dBFS: {max_loudness_dbfs:.2f} dBFS).")
            
        # 4. Duration check - already handled in process_audio_pipeline, but can add a message here
        if duration < 2.0: # Minimum duration for meaningful speech, adjust as needed
            overall_quality = "warn" if overall_quality == "pass" else overall_quality
            quality_score *= 0.8 # Penalty for short duration
            messages.append(f"WARN: Thời lượng âm thanh ngắn ({duration:.2f}s).")
        elif duration > 60.0: # Maximum duration, adjust as needed
            overall_quality = "warn" if overall_quality == "pass" else overall_quality
            quality_score *= 0.9 # Slight penalty for very long audio, might indicate issues
            messages.append(f"WARN: Thời lượng âm thanh dài ({duration:.2f}s).")
        else:
            messages.append(f"PASS: Thời lượng âm thanh chấp nhận được ({duration:.2f}s).")


        # Final quality score adjustment (e.g., scale to 100)
        quality_score = round(quality_score * 100, 2)
        if quality_score < 30 and overall_quality != "reject": # If score is very low but not rejected yet
            overall_quality = "reject"
            messages.insert(0, "REJECT: Điểm chất lượng tổng thể quá thấp.")
        elif quality_score < 60 and overall_quality == "pass": # If score is mediocre but still "pass"
             overall_quality = "warn"
             messages.insert(0, "WARN: Điểm chất lượng tổng thể ở mức trung bình.")

        return AudioQualityReport(overall_quality=overall_quality, quality_score=quality_score, messages=messages)

    async def process_audio_pipeline(self, audio_urls: List[str]) -> Tuple[str, float, AudioQualityReport]:
        """
        Orchestrates the audio preprocessing pipeline:
        1. Downloads each audio from URL.
        2. Processes each audio (WAV, mono, 16kHz, VAD only).
        3. Concatenates all processed audios into a single WAV file.
        4. Performs quality checks on the merged audio.
        5. Uploads the final WAV file to object storage.
        6. Returns the URL of the processed audio, its total duration, and the quality report.
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

            # 1. & 2. Download and process each audio (standardize format and VAD only)
            for i, url in enumerate(audio_urls):
                downloaded_file = temp_dir_path / f"downloaded_{i}.tmp"
                processed_file = temp_dir_path / f"processed_{i}.wav"

                await audio_utils.download_audio(url, str(downloaded_file))
                downloaded_paths.append(downloaded_file)

                # Process single audio (format standardization and VAD only)
                await audio_utils.process_single_audio(str(downloaded_file), str(processed_file))
                processed_paths.append(processed_file)

            # 3. Concatenate all processed audios
            merged_audio_temp_file = temp_dir_path / "merged_output.wav"
            total_duration = await audio_utils.concatenate_audios(
                [str(p) for p in processed_paths], str(merged_audio_temp_file)
            )

            # 4. Perform quality checks on the merged audio
            quality_report = await self._perform_audio_quality_checks(str(merged_audio_temp_file), total_duration)

            # 5. Generate a unique filename for the publicly accessible static file
            unique_filename = f"{uuid.uuid4()}.wav"
            final_static_filepath = settings.STATIC_FILES_DIR / unique_filename

            # Move the merged audio file to the static files directory
            os.rename(merged_audio_temp_file, final_static_filepath)
            logger.info(f"Moved merged audio to static directory: {final_static_filepath}")

            # Construct the absolute URL for the processed audio file
            processed_audio_url = f"{settings.VOICE_SERVICE_BASE_URL}/static/{unique_filename}"
            logger.info(f"VOICE_SERVICE_BASE_URL: '{settings.VOICE_SERVICE_BASE_URL}', unique_filename: '{unique_filename}'")
            logger.info(f"Audio preprocessing pipeline completed. Final URL: {processed_audio_url}, Duration: {total_duration:.2f}s, Quality: {quality_report.overall_quality}")
            return processed_audio_url, total_duration, quality_report

        except Exception as e:
            logger.error(f"Error during audio preprocessing pipeline: {e}")
            raise
        finally:
            # Clean up temporary downloaded and processed files in the temp_dir_path
            if temp_dir_path and temp_dir_path.exists():
                for path in downloaded_paths + processed_paths:  # merged_audio_temp_file is moved, not deleted here
                    if path.exists():
                        os.remove(path)
                shutil.rmtree(temp_dir_path)
                logger.info(f"Cleaned up temporary directory: {temp_dir_path}")


_preprocess_service_instance: PreprocessService = None


def get_preprocess_service() -> PreprocessService:
    global _preprocess_service_instance
    if _preprocess_service_instance is None:
        _preprocess_service_instance = PreprocessService()
    return _preprocess_service_instance
