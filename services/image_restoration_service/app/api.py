import logging
from typing import Any, Dict
from uuid import UUID, uuid4
from io import BytesIO
import base64

from fastapi import APIRouter, BackgroundTasks, HTTPException, UploadFile, File
from PIL import Image

from app.models.job import RestorationJob, RestorationStatus
from app.services.replicate_service import get_replicate_service, ReplicateService
from app.services.backend_api_service import get_backend_api_service, BackendApiService

router = APIRouter()

# Setup logger
logger = logging.getLogger(__name__)

# Constants
MAX_PIXELS_FOR_PREPROCESSING = 2096704 # Example max pixels for preprocessing

# In-memory store for jobs
# In a real application, this would be a database (Redis, PostgreSQL, etc.)
jobs: Dict[UUID, RestorationJob] = {}

@router.post("/preprocess-image")
async def preprocess_image_endpoint(file: UploadFile = File(...)):
    """
    Receives an image file, resizes it if its pixel count exceeds a maximum,
    and returns the processed image as a base64 string.
    """
    logger.info("Received file %s for preprocessing.", file.filename)
    if not file.content_type.startswith("image/"):
        raise HTTPException(status_code=400, detail="Only image files are allowed.")

    try:
        content = await file.read()
        img_data = BytesIO(content)
        img = Image.open(img_data).convert("RGB")

        width, height = img.size
        current_pixels = width * height
        is_resized = False

        if current_pixels > MAX_PIXELS_FOR_PREPROCESSING:
            logger.warning("Image dimensions %sx%s (%s pixels) exceed max_pixels %s. Resizing...",
                           width, height, current_pixels, MAX_PIXELS_FOR_PREPROCESSING)
            
            # Calculate new dimensions
            scale_factor = (MAX_PIXELS_FOR_PREPROCESSING / current_pixels)**0.5
            new_width = int(width * scale_factor)
            new_height = int(height * scale_factor)

            img = img.resize((new_width, new_height), Image.LANCZOS)
            is_resized = True
            logger.info("Image resized to %sx%s (%s pixels).", new_width, new_height, new_width * new_height)

        buffered = BytesIO()
        img.save(buffered, format="JPEG") # Use JPEG for base64 encoding
        base64_image = base64.b64encode(buffered.getvalue()).decode()
        
        return {"processedImageBase64": f"data:image/jpeg;base64,{base64_image}", "is_resized": is_resized}
    except Exception as e:
        logger.error("Error during image preprocessing: %s", e, exc_info=True)
        raise HTTPException(status_code=500, detail=f"Error processing image: {e}")


@router.post("/restore", response_model=RestorationJob)
async def start_restoration_job(
    image_data: Dict[str, Any], # Changed from str to Any to accommodate boolean
    background_tasks: BackgroundTasks
):
    """
    Starts an asynchronous image restoration job.
    The response is returned immediately with a job_id.
    """
    image_url = image_data.get("imageUrl")
    use_codeformer = image_data.get("useCodeformer", False) # Get optional useCodeformer flag
    if not image_url:
        raise HTTPException(status_code=400, detail="imageUrl is required")

    logger.info("Starting restoration job for image URL: %s", image_url)
    job_id = uuid4()
    job = RestorationJob(
        job_id=job_id, status=RestorationStatus.PROCESSING, original_url=image_url
    )
    jobs[job_id] = job

    # Run the restoration pipeline directly and await its completion
    final_job = await _run_restoration_pipeline(
        job_id,
        image_url,
        get_replicate_service(), # Get the instance
        get_backend_api_service(), # Get the instance
        use_codeformer # Pass use_codeformer
    )
    logger.info("Restoration job %s completed. Final status: %s", final_job.job_id, final_job.status)
    return final_job


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
    backend_api_service: BackendApiService, # Injected dependency
    use_codeformer: bool = False
):
    """
    Internal function to run the image restoration pipeline.
    Updates the job status and results in the global jobs dictionary.
    """
    logger.info("Running restoration pipeline for job %s, image: %s", job_id, image_url)
    job = jobs[job_id]
    try:
        logger.info("Calling ReplicateService for image restoration for job %s", job_id)
        result = await replicate_service.restore_image_pipeline(image_url, use_codeformer=use_codeformer)
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
        
        # Notify backend about the final status - NOT needed if not background job
        logger.info("Backend notification removed as job is not run in background for job %s.", job_id)
        # await backend_api_service.update_image_restoration_job_status(
        #     job.job_id,
        #     job.status,
        #     job.restored_url,
        #     job.error
        # )
        # logger.info("Backend notification completed for job %s.", job_id)
    return job
