import logging
import httpx
import json
from uuid import UUID
from typing import Optional
from app.config import settings
from app.models.job import RestorationStatus

# Setup logger
logger = logging.getLogger(__name__)

class BackendApiService:
    def __init__(self):
        self.client = httpx.AsyncClient()
        self.backend_api_url = settings.BACKEND_API_URL

    async def update_image_restoration_job_status(
        self, job_id: UUID, status: RestorationStatus, restored_url: Optional[str], error_message: Optional[str]
    ):
        if not self.backend_api_url:
            logger.warning("BACKEND_API_URL is not set. Skipping backend status update for job %s.", job_id)
            return

        endpoint = f"{self.backend_api_url}/api/image-restoration-jobs/{job_id}/status"
        payload = {
            "jobId": str(job_id), # Ensure it's a string for the backend API
            "status": status.value,
            "restoredImageUrl": restored_url,
            "errorMessage": error_message,
        }
        headers = {"Content-Type": "application/json"}
        logger.info("Attempting to update backend status for job %s at %s with payload: %s", job_id, endpoint, payload)

        try:
            # The backend expects a PATCH, but httpx uses patch()
            response = await self.client.patch(endpoint, headers=headers, content=json.dumps(payload))
            response.raise_for_status()  # Raise an exception for 4xx or 5xx responses
            logger.info("Successfully updated backend for job %s. Status: %s", job_id, response.status_code)
        except httpx.RequestError as e:
            logger.error("ERROR: An error occurred while requesting backend %s for job %s: %s", e.request.url, job_id, e, exc_info=True)
        except httpx.HTTPStatusError as e:
            logger.error("ERROR: Received error response %s from backend for job %s: %s", e.response.status_code, job_id, e.response.text)
        except Exception as e:
            logger.error("ERROR: An unexpected error occurred during backend status update for job %s: %s", job_id, e, exc_info=True)

_backend_api_service_instance: Optional[BackendApiService] = None

def get_backend_api_service() -> BackendApiService:
    global _backend_api_service_instance
    if _backend_api_service_instance is None:
        _backend_api_service_instance = BackendApiService()
    return _backend_api_service_instance