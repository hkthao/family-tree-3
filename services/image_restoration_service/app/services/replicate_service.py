import logging
import replicate
import httpx
from typing import List, Optional
from app.config import settings

# Setup logger
logger = logging.getLogger(__name__)

class ReplicateService:
    def __init__(self):
        # Replicate client is initialized with REPLICATE_API_TOKEN from environment variables
        # No explicit token passing needed here if the env var is set
        if not settings.REPLICATE_API_TOKEN:
            raise ValueError("REPLICATE_API_TOKEN is not set in environment variables.")

    async def _run_prediction(self, model_version: str, inputs: dict) -> List[str]:
        """
        Helper to run a Replicate prediction and return the output URLs.
        Handles potential list output.
        """
        logger.info("Running Replicate prediction for model %s with inputs: %s", model_version, inputs)
        try:
            output = await replicate.async_run(
                model_version,
                input=inputs
            )
            if isinstance(output, list):
                logger.info("Replicate prediction for %s successful. Output: %s", model_version, output)
                return [str(url) for url in output]
            elif output:
                logger.info("Replicate prediction for %s successful. Output: %s", model_version, output)
                return [str(output)]
            logger.warning("Replicate prediction for %s returned no output.", model_version)
            return []
        except replicate.exceptions.ReplicateError as e:
            logger.error("Replicate API error during prediction for %s: %s", model_version, e)
            raise ValueError(f"Replicate API error: {e}")
        except httpx.ConnectError as e:
            logger.error("Network connection error to Replicate during prediction for %s: %s", model_version, e)
            raise ValueError(f"Network connection error to Replicate: {e}")
        except Exception as e:
            logger.error("An unexpected error occurred during Replicate prediction for %s: %s", model_version, e, exc_info=True)
            raise ValueError(f"An unexpected error occurred during Replicate prediction: {e}")

    async def gfpgan_restore_face(self, image_url: str) -> Optional[str]:
        """
        Restores faces using TencentARC's GFPGAN model.
        Returns the URL of the restored image.
        """
        logger.info("Starting GFPGAN face restoration for image: %s", image_url)
        model_version = "tencentarc/gfpgan:0fbacf7afc6c144e5be9767cff80f25aff23e52b0708f17e20f9879b2f21516c"
        inputs = {
            "img": image_url,
            "version": "v1.4",
            "scale": 2,
        }
        output_urls = await self._run_prediction(model_version, inputs)
        restored_url = output_urls[0] if output_urls else None
        logger.info("GFPGAN restoration completed. Restored URL: %s", restored_url)
        return restored_url

    async def real_esrgan_upscale(self, image_url: str) -> Optional[str]:
        """
        Upscales an image using nightmareai's Real-ESRGAN model.
        Returns the URL of the upscaled image.
        """
        logger.info("Starting Real-ESRGAN upscale for image: %s", image_url)
        model_version = "nightmareai/real-esrgan"
        inputs = {
            "image": image_url,
            "scale": 2,
        }
        output_urls = await self._run_prediction(model_version, inputs)
        upscaled_url = output_urls[0] if output_urls else None
        logger.info("Real-ESRGAN upscale completed. Upscaled URL: %s", upscaled_url)
        return upscaled_url

    async def restore_image_pipeline(self, original_image_url: str) -> dict:
        """
        Executes the full image restoration pipeline: GFPGAN (face) -> Real-ESRGAN (upscale).
        Returns a dictionary with restoration details.
        """
        logger.info("Starting full image restoration pipeline for original image: %s", original_image_url)
        restored_face_url = None
        upscaled_url = None
        pipeline_steps = []

        # Step 1: Face Restoration with GFPGAN
        try:
            logger.info("Pipeline Step 1: GFPGAN face restoration.")
            restored_face_url = await self.gfpgan_restore_face(original_image_url)
            if restored_face_url:
                pipeline_steps.append("GFPGAN")
                logger.info("GFPGAN restored image URL: %s", restored_face_url)
            else:
                logger.warning("GFPGAN did not return a restored image URL. Skipping upscale for %s.", original_image_url)
                return {
                    "originalUrl": original_image_url,
                    "restoredUrl": None,
                    "pipeline": pipeline_steps,
                    "status": "failed",
                    "error": "GFPGAN did not return a valid image URL."
                }
        except ValueError as e:
            logger.error("GFPGAN restoration failed for %s: %s", original_image_url, e)
            return {
                "originalUrl": original_image_url,
                "restoredUrl": None,
                "pipeline": [],
                "status": "failed",
                "error": f"GFPGAN failed: {e}"
            }

        # Step 2: Upscale Image with Real-ESRGAN (using GFPGAN output)
        if restored_face_url:
            try:
                logger.info("Pipeline Step 2: Real-ESRGAN upscale using GFPGAN output: %s", restored_face_url)
                upscaled_url = await self.real_esrgan_upscale(restored_face_url)
                if upscaled_url:
                    pipeline_steps.append("Real-ESRGAN")
                    logger.info("Real-ESRGAN upscaled image URL: %s", upscaled_url)
                else:
                    logger.warning("Real-ESRGAN did not return an upscaled image URL for %s. Returning GFPGAN result.", original_image_url)
                    # If upscale fails but face restoration succeeded, return the GFPGAN result
                    return {
                        "originalUrl": original_image_url,
                        "restoredUrl": restored_face_url,
                        "pipeline": pipeline_steps,
                        "status": "completed", # Completed up to GFPGAN
                        "error": "Real-ESRGAN did not return a valid image URL."
                    }
            except ValueError as e:
                logger.error("Real-ESRGAN upscaling failed for %s: %s. Returning GFPGAN result.", original_image_url, e)
                # If upscale fails but face restoration succeeded, return the GFPGAN result
                return {
                    "originalUrl": original_image_url,
                    "restoredUrl": restored_face_url,
                    "pipeline": pipeline_steps,
                    "status": "completed", # Completed up to GFPGAN
                    "error": f"Real-ESRGAN failed: {e}"
                }
        
        final_status = "completed" if (restored_face_url or upscaled_url) else "failed"
        logger.info("Full pipeline completed for %s. Final status: %s, Restored URL: %s", 
                    original_image_url, final_status, upscaled_url if upscaled_url else restored_face_url)
        return {
            "originalUrl": original_image_url,
            "restoredUrl": upscaled_url if upscaled_url else restored_face_url, # Return upscaled if available, else gfpgan
            "pipeline": pipeline_steps,
            "status": final_status
        }

_replicate_service_instance: Optional[ReplicateService] = None

def get_replicate_service() -> ReplicateService:
    global _replicate_service_instance
    if _replicate_service_instance is None:
        _replicate_service_instance = ReplicateService()
    return _replicate_service_instance
