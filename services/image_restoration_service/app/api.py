from typing import Dict
from uuid import UUID, uuid4

from fastapi import APIRouter, BackgroundTasks, HTTPException
from app.models.job import RestorationJob, RestorationStatus
from app.services.replicate_service import replicate_service

router = APIRouter()

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

    job_id = uuid4()
    job = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )
    jobs[job_id] = job

    # Run the restoration pipeline in the background
    background_tasks.add_task(_run_restoration_pipeline, job_id, image_url)

    return job


@router.get("/restore/{job_id}", response_model=RestorationJob)
async def get_restoration_job_status(job_id: UUID):
    """
    Checks the status of an image restoration job.
    """
    job = jobs.get(job_id)
    if not job:
        raise HTTPException(status_code=404, detail="Job not found")
    return job


async def _run_restoration_pipeline(job_id: UUID, image_url: str):
    """
    Internal function to run the image restoration pipeline.
    Updates the job status and results in the global jobs dictionary.
    """
    job = jobs[job_id]
    try:
        result = await replicate_service.restore_image_pipeline(image_url)
        job.restored_url = result.get("restoredUrl")
        job.pipeline = result.get("pipeline", [])
        job.status = result.get("status", RestorationStatus.COMPLETED)
        job.error = result.get("error")
    except Exception as e:
        job.status = RestorationStatus.FAILED
        job.error = str(e)
    finally:
        # Ensure job is marked as completed or failed
        if job.status == RestorationStatus.PROCESSING:
            job.status = RestorationStatus.FAILED
            job.error = job.error or "Unknown error occurred during processing."
