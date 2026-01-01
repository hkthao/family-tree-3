import httpx
import json
from uuid import UUID
from typing import Optional
from app.config import settings
from app.models.job import RestorationStatus

class BackendApiService:
    def __init__(self):
        self.client = httpx.AsyncClient()
        self.backend_api_url = settings.BACKEND_API_URL

    async def update_image_restoration_job_status(
        self, job_id: UUID, status: RestorationStatus, restored_url: Optional[str], error_message: Optional[str]
    ):
        if not self.backend_api_url:
            print("WARNING: BACKEND_API_URL is not set. Skipping backend status update.")
            return

        endpoint = f"{self.backend_api_url}/api/image-restoration-jobs/{job_id}/status"
        payload = {
            "jobId": str(job_id), # Ensure it's a string for the backend API
            "status": status.value,
            "restoredImageUrl": restored_url,
            "errorMessage": error_message,
        }
        headers = {"Content-Type": "application/json"}

        try:
            # The backend expects a PATCH, but httpx uses patch()
            response = await self.client.patch(endpoint, headers=headers, content=json.dumps(payload))
            response.raise_for_status()  # Raise an exception for 4xx or 5xx responses
            print(f"Successfully updated backend for job {job_id}. Status: {response.status_code}")
        except httpx.RequestError as e:
            print(f"ERROR: An error occurred while requesting {e.request.url!r}: {e}")
        except httpx.HTTPStatusError as e:
            print(f"ERROR: Received error response {e.response.status_code} -- {e.response.text}")
        except Exception as e:
            print(f"ERROR: An unexpected error occurred during backend status update: {e}")

_backend_api_service_instance: Optional[BackendApiService] = None

def get_backend_api_service() -> BackendApiService:
    global _backend_api_service_instance
    if _backend_api_service_instance is None:
        _backend_api_service_instance = BackendApiService()
    return _backend_api_service_instance