import os
from dotenv import load_dotenv
from loguru import logger

load_dotenv() # Load environment variables from .env file

class Settings:
    # Replicate
    REPLICATE_API_TOKEN: str = os.getenv("REPLICATE_API_TOKEN", "")

    # S3/R2 Compatible Storage
    AWS_ACCESS_KEY_ID: str = os.getenv("AWS_ACCESS_KEY_ID", "")
    AWS_SECRET_ACCESS_KEY: str = os.getenv("AWS_SECRET_ACCESS_KEY", "")
    AWS_ENDPOINT_URL: str = os.getenv("AWS_ENDPOINT_URL", "")
    AWS_REGION: str = os.getenv("AWS_REGION", "auto") # Cloudflare R2 often uses "auto"
    AWS_BUCKET_NAME: str = os.getenv("AWS_BUCKET_NAME", "")

settings = Settings()

if not settings.REPLICATE_API_TOKEN:
    logger.warning("REPLICATE_API_TOKEN environment variable is not set. Replicate API calls will fail.")
if not all([settings.AWS_ACCESS_KEY_ID, settings.AWS_SECRET_ACCESS_KEY,
            settings.AWS_ENDPOINT_URL, settings.AWS_BUCKET_NAME]):
    logger.warning("One or more S3/R2 storage environment variables (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_ENDPOINT_URL, AWS_BUCKET_NAME) are not set. Storage operations will fail.")
