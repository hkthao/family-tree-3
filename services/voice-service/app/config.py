from pathlib import Path
import os
from dotenv import load_dotenv
from loguru import logger


load_dotenv()  # Load environment variables from .env file


class Settings:
    # Replicate
    REPLICATE_API_TOKEN: str = os.getenv("REPLICATE_API_TOKEN", "")

    # Static File Serving
    STATIC_FILE_LIFETIME_SECONDS: int = int(os.getenv("STATIC_FILE_LIFETIME_SECONDS", 3600))  # Default to 1 hour
    VOICE_SERVICE_BASE_URL: str = os.getenv("VOICE_SERVICE_BASE_URL", "http://localhost:8000")  # Base URL for the voice service
    STATIC_FILES_DIR: Path = Path("app/static_files")  # Directory for serving temporary audio files


settings = Settings()


settings.STATIC_FILES_DIR.mkdir(parents=True, exist_ok=True)  # Ensure directory exists

if not settings.REPLICATE_API_TOKEN:
    logger.warning("REPLICATE_API_TOKEN environment variable is not set. Replicate API calls will fail.")
if not settings.VOICE_SERVICE_BASE_URL:
    logger.warning("VOICE_SERVICE_BASE_URL environment variable is not set. Relative URLs will be returned, which might cause issues for external consumers.")
