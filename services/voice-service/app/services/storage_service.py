import boto3
import os
from loguru import logger
from app.config import settings
from botocore.client import Config # Import Config to set s3 path style


class S3StorageService:
    def __init__(self):
        self.s3_client = None
        self.bucket_name = settings.AWS_BUCKET_NAME

        if not all([settings.AWS_ACCESS_KEY_ID, settings.AWS_SECRET_ACCESS_KEY,
                    settings.AWS_ENDPOINT_URL, settings.AWS_BUCKET_NAME]):
            error_msg = "S3/R2 storage credentials not fully configured. Storage operations cannot be initialized."
            logger.error(error_msg)
            raise ValueError(error_msg)

        try:
            self.s3_client = boto3.client(
                's3',
                endpoint_url=settings.AWS_ENDPOINT_URL,
                aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY,
                region_name=settings.AWS_REGION,
                config=Config(s3={'addressing_style': 'path'}) # Ensure path-style addressing for R2
            )
            logger.info("S3StorageService initialized successfully.")
        except Exception as e:
            logger.error(f"Error initializing S3StorageService: {e}")
            raise ValueError(f"Failed to initialize S3StorageService: {e}")

    async def upload_file(self, file_path: str, object_name: str, content_type: str = "audio/wav") -> str:
        """
        Uploads a file to the S3-compatible storage.
        Returns the URL of the uploaded file.
        """
        if not self.s3_client:
            raise RuntimeError("S3StorageService not initialized due to missing credentials.")

        logger.info(f"Uploading file {file_path} to S3 bucket {self.bucket_name} as {object_name} (Content-Type: {content_type})")
        try:
            with open(file_path, "rb") as f:
                self.s3_client.upload_fileobj(
                    f,
                    self.bucket_name,
                    object_name,
                    ExtraArgs={'ContentType': content_type}
                )
            file_url = self._get_file_url(object_name)
            logger.info(f"Successfully uploaded {object_name}. URL: {file_url}")
            return file_url
        except Exception as e:
            logger.error(f"Error uploading file {file_path} to S3: {e}")
            raise ValueError(f"Failed to upload file {file_path} to S3: {e}")

    def _get_file_url(self, object_name: str) -> str:
        """
        Constructs the public URL for an uploaded file.
        """
        # For Cloudflare R2, the endpoint URL usually contains the bucket name
        # e.g., https://<account_id>.r2.cloudflarestorage.com/<bucket_name>/<object_name>
        # If the endpoint doesn't include the bucket name, adjust accordingly.
        if self.bucket_name not in settings.AWS_ENDPOINT_URL:
            return f"{settings.AWS_ENDPOINT_URL}/{self.bucket_name}/{object_name}"
        return f"{settings.AWS_ENDPOINT_URL}/{object_name}"


_s3_storage_service_instance: S3StorageService = None

def get_s3_storage_service() -> S3StorageService:
    global _s3_storage_service_instance
    if _s3_storage_service_instance is None:
        _s3_storage_service_instance = S3StorageService()
    return _s3_storage_service_instance
