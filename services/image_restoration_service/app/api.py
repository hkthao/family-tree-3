import logging
from typing import Dict
from uuid import UUID, uuid4

from fastapi import APIRouter, BackgroundTasks, HTTPException
from app.models.job import RestorationJob, RestorationStatus
from app.services.replicate_service import get_replicate_service, ReplicateService
from app.services.backend_api_service import get_backend_api_service, BackendApiService

router = APIRouter()

# Setup logger
logger = logging.getLogger(__name__)

# In-memory store for jobs
# In a real application, this would be a database (Redis, PostgreSQL, etc.)
jobs: Dict[UUID, RestorationJob] = {}


@router.post("/restore", response_model=RestorationJob)
async def start_restoration_job(
    image_data: Dict[str, str], background_tasks: BackgroundTasks
):
    """
    Starts an asynchronous image restoration job.
    The response is returned immediately with a job_id.
    """
    image_url = image_data.get("imageUrl")
    if not image_url:
        raise HTTPException(status_code=400, detail="imageUrl is required")

    logger.info("Starting restoration job for image URL: %s", image_url)
    job_id = uuid4()
    job = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )
    jobs[job_id] = job

    # Run the restoration pipeline in the background
    background_tasks.add_task(
        _run_restoration_pipeline,
        job_id,
        image_url,
        get_replicate_service(), # Get the instance
        get_backend_api_service() # Get the instance
    )
    logger.info("Restoration job %s started in background.", job_id)
    return job


@router.get("/restore/{job_id}", response_model=RestorationJob)
async def get_restoration_job_status(job_id: UUID):
    """
    Checks the status of an image restoration job.
    """
    job = jobs.get(job_id)
    if not job:
        logger.warning("Job %s not found.", job_id)
        raise HTTPException(status_code=404, detail="Job not found")
    logger.info("Fetching status for job %s. Current status: %s", job_id, job.status)
    return job


async def _run_restoration_pipeline(
    job_id: UUID,
    image_url: str,
    replicate_service: ReplicateService, # Injected dependency
    backend_api_service: BackendApiService # Injected dependency
):
    """
    Internal function to run the image restoration pipeline.
    Updates the job status and results in the global jobs dictionary.
    """
    logger.info("Running restoration pipeline for job %s, image: %s", job_id, image_url)
    job = jobs[job_id]
    try:
        logger.info("Calling ReplicateService for image restoration for job %s", job_id)
        result = await replicate_service.restore_image_pipeline(image_url)
        logger.info("ReplicateService returned result for job %s: %s", job_id, result)

        job.restored_url = result.get("restoredUrl")
        job.pipeline = result.get("pipeline", [])
        status_from_replicate = result.get("status", RestorationStatus.COMPLETED.value)
        job.status = RestorationStatus(status_from_replicate)
        job.error = result.get("error")
        logger.info("Job %s status updated internally to %s", job_id, job.status)
    except Exception as e:
        logger.error("Error during restoration pipeline for job %s: %s", job_id, e, exc_info=True)
        job.status = RestorationStatus.FAILED
        job.error = str(e)
    finally:
        # Ensure job is marked as completed or failed
        if job.status == RestorationStatus.PROCESSING:
            job.status = RestorationStatus.FAILED
            job.error = job.error or "Unknown error occurred during processing."
            logger.warning("Job %s was still in PROCESSING, marking as FAILED in finally block.", job_id)
        
        # Notify backend about the final status
        logger.info("Notifying backend of final status for job %s. Status: %s, Restored URL: %s, Error: %s",
                    job.job_id, job.status, job.restored_url, job.error)
        await backend_api_service.update_image_restoration_job_status(
            job.job_id,
            job.status,
            job.restored_url,
            job.error
        )
        logger.info("Backend notification completed for job %s.", job_id)
