import replicate
import httpx
from loguru import logger
from typing import Optional
from app.config import settings


class ReplicateService:
    def __init__(self):
        if not settings.REPLICATE_API_TOKEN:
            logger.error("REPLICATE_API_TOKEN is not set in environment variables.")
            raise ValueError("REPLICATE_API_TOKEN is not set in environment variables.")

        replicate.api_token = settings.REPLICATE_API_TOKEN
        # XTTS model version from Coqui
        # Ensure this is the correct and desired model version
        self.xtts_model_version = "lucataco/xtts-v2:684bc3855b37866c0c65add2ff39c78f3dea3f4ff103a436465326e0f438d55e"
        logger.info("ReplicateService initialized with XTTS model version: %s", self.xtts_model_version)

    async def generate_voice(self, speaker_wav_url: str, text: str, language: str) -> str:
        """
        Generates voice audio from text using Replicate's XTTS model.
        Returns the URL of the generated audio.
        """
        logger.info("Calling Replicate XTTS for voice generation. Speaker WAV: %s, Language: %s", speaker_wav_url, language)
        try:
            # Supported languages for XTTS-v2 based on Replicate documentation
            supported_languages = ["en", "es", "fr", "de", "it", "pt", "pl", "tr", "ru", "nl", "cs", "ar", "zh", "hu", "ko", "hi", "ja"]

            if language not in supported_languages:
                raise ValueError(f"Language '{language}' is not supported by the XTTS model. Supported languages are: {', '.join(supported_languages)}")

            # Replicate model input parameters for XTTS-v2
            inputs = {
                "speaker_wav": speaker_wav_url,
                "text": text,
                "language": language,
            }
            logger.debug("Replicate XTTS inputs: %s", inputs)

            output = await replicate.async_run(
                self.xtts_model_version,
                input=inputs
            )

            if isinstance(output, str) and output:
                logger.info("Replicate XTTS call successful. Generated audio URL: %s", output)
                return output
            else:
                logger.error("Replicate XTTS returned an unexpected output format or empty output: %s", output)
                raise ValueError("Replicate XTTS failed to generate valid audio URL.")

        except replicate.exceptions.ReplicateError as e:
            logger.error(f"Replicate API error during XTTS generation: {e}")
            raise ValueError(f"Replicate API error: {e}")
        except httpx.RequestError as e:
            logger.error(f"Network connection error to Replicate during XTTS generation: {e}")
            raise ValueError(f"Network connection error to Replicate: {e}")
        except Exception as e:
            logger.error(f"An unexpected error occurred during Replicate XTTS generation: {e}", exc_info=True)
            raise ValueError(f"An unexpected error occurred during Replicate XTTS generation: {e}")


_replicate_service_instance: Optional[ReplicateService] = None


def get_replicate_service() -> ReplicateService:
    global _replicate_service_instance
    if _replicate_service_instance is None:
        _replicate_service_instance = ReplicateService()
    return _replicate_service_instance
