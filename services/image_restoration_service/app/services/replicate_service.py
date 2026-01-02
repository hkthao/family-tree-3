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

    async def restore_image_pipeline(self, original_image_url: str, use_codeformer: bool = False) -> dict:
        """
        Executes the full image restoration pipeline based on user's defined steps.
        1. Face Restore (GFPGAN or Codeformer - if implemented)
        2. Clean & Enhance (Real-ESRGAN) - applied to the face-restored image
        Returns a dictionary with restoration details.
        """
        logger.info("Starting full image restoration pipeline for original image: %s, use_codeformer: %s", original_image_url, use_codeformer)
        restored_face_url = original_image_url # Image URL after face restoration step
        pipeline_steps = []
        error_message = None
        final_status = "completed"

        # Step 1: Face Restore (GFPGAN or Codeformer)
        if use_codeformer:
            logger.info("Skipping GFPGAN. 'use_codeformer' is True. Codeformer implementation is pending for face restoration.")
            # TODO: Implement Codeformer face restoration logic here
            # For now, if use_codeformer is True, face restoration is effectively skipped,
            # and the original image will proceed to the upscaling step.
            pipeline_steps.append("Codeformer (Face Restore) - Pending/Skipped")
        else:
            try:
                logger.info("Pipeline Step 1: GFPGAN for Face Restore on original image: %s", original_image_url)
                gfpgan_output_url = await self.gfpgan_restore_face(original_image_url)
                if gfpgan_output_url:
                    restored_face_url = gfpgan_output_url
                    pipeline_steps.append("GFPGAN (Face Restore)")
                    logger.info("GFPGAN (Face Restore) completed. Restored face URL: %s", restored_face_url)
                else:
                    logger.warning("GFPGAN (Face Restore) did not return a restored image for %s. Proceeding with original image for upscaling.", original_image_url)
                    error_message = "GFPGAN (Face Restore) did not return a valid image URL."
            except ValueError as e:
                logger.error("GFPGAN (Face Restore) failed for %s: %s. Proceeding with original image for upscaling.", original_image_url, e)
                error_message = f"GFPGAN (Face Restore) failed: {e}"
        
        # Step 2: Clean & Enhance (toàn ảnh) - using Real-ESRGAN on the face-restored or original image
        final_restored_url = None
        try:
            logger.info("Pipeline Step 2: Real-ESRGAN for Clean & Enhance on image: %s", restored_face_url)
            esrgan_output_url = await self.real_esrgan_upscale(restored_face_url)
            if esrgan_output_url:
                final_restored_url = esrgan_output_url
                pipeline_steps.append("Real-ESRGAN (Clean & Enhance)")
                logger.info("Real-ESRGAN (Clean & Enhance) completed. Final URL: %s", final_restored_url)
            else:
                logger.warning("Real-ESRGAN (Clean & Enhance) did not return an output for %s. Final restored URL is the previous step's output if any.", restored_face_url)
                final_status = "failed"
                error_message = "Real-ESRGAN (Clean & Enhance) did not return a valid image URL."
        except ValueError as e:
            logger.error("Real-ESRGAN (Clean & Enhance) failed for %s: %s", restored_face_url, e)
            final_status = "failed"
            error_message = f"Real-ESRGAN (Clean & Enhance) failed: {e}"

        logger.info("Full pipeline completed for %s. Final status: %s, Restored URL: %s", 
                    original_image_url, final_status, final_restored_url if final_restored_url else original_image_url)
        return {
            "originalUrl": original_image_url,
            "restoredUrl": final_restored_url, # This will be None if ESRGAN failed
            "pipeline": pipeline_steps,
            "status": final_status,
            "error": error_message
        }

_replicate_service_instance: Optional[ReplicateService] = None

def get_replicate_service() -> ReplicateService:
    global _replicate_service_instance
    if _replicate_service_instance is None:
        _replicate_service_instance = ReplicateService()
    return _replicate_service_instance
