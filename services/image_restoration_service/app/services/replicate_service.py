import replicate
import httpx
from typing import List, Optional
from app.config import settings

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
        try:
            output = await replicate.async_run(
                model_version,
                input=inputs
            )
            # Replicate can return a list of URLs or a single URL
            if isinstance(output, list):
                return [str(url) for url in output]
            elif output:
                return [str(output)]
            return []
        except replicate.exceptions.ReplicateError as e:
            raise ValueError(f"Replicate API error: {e}")
        except httpx.ConnectError as e:
            raise ValueError(f"Network connection error to Replicate: {e}")
        except Exception as e:
            raise ValueError(f"An unexpected error occurred during Replicate prediction: {e}")

    async def gfpgan_restore_face(self, image_url: str) -> Optional[str]:
        """
        Restores faces using TencentARC's GFPGAN model.
        Returns the URL of the restored image.
        """
        model_version = "tencentarc/gfpgan:9283608cc6b7be6b65a8e44983db012355f41fa0be9861609f3e4dcc23999ef2"
        inputs = {
            "img": image_url,
            "version": "v1.4",
            "scale": 2,
        }
        output_urls = await self._run_prediction(model_version, inputs)
        return output_urls[0] if output_urls else None

    async def real_esrgan_upscale(self, image_url: str) -> Optional[str]:
        """
        Upscales an image using nightmareai's Real-ESRGAN model.
        Returns the URL of the upscaled image.
        """
        model_version = "nightmareai/real-esrgan:422045c7477197c83884803157c10b24d77759d0421685973bc5e52f1e695d7f"
        inputs = {
            "image": image_url,
            "scale": 2,
        }
        output_urls = await self._run_prediction(model_version, inputs)
        return output_urls[0] if output_urls else None

    async def restore_image_pipeline(self, original_image_url: str) -> dict:
        """
        Executes the full image restoration pipeline: GFPGAN (face) -> Real-ESRGAN (upscale).
        Returns a dictionary with restoration details.
        """
        restored_face_url = None
        upscaled_url = None
        pipeline_steps = []

        # Step 1: Face Restoration with GFPGAN
        try:
            restored_face_url = await self.gfpgan_restore_face(original_image_url)
            if restored_face_url:
                pipeline_steps.append("GFPGAN")
                print(f"GFPGAN restored image URL: {restored_face_url}")
            else:
                print("GFPGAN did not return a restored image URL. Skipping upscale.")
                return {
                    "originalUrl": original_image_url,
                    "restoredUrl": None,
                    "pipeline": pipeline_steps,
                    "status": "failed",
                    "error": "GFPGAN did not return a valid image URL."
                }
        except ValueError as e:
            print(f"GFPGAN restoration failed: {e}")
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
                upscaled_url = await self.real_esrgan_upscale(restored_face_url)
                if upscaled_url:
                    pipeline_steps.append("Real-ESRGAN")
                    print(f"Real-ESRGAN upscaled image URL: {upscaled_url}")
                else:
                    print("Real-ESRGAN did not return an upscaled image URL.")
                    # If upscale fails but face restoration succeeded, return the GFPGAN result
                    return {
                        "originalUrl": original_image_url,
                        "restoredUrl": restored_face_url,
                        "pipeline": pipeline_steps,
                        "status": "completed", # Completed up to GFPGAN
                        "error": "Real-ESRGAN did not return a valid image URL."
                    }
            except ValueError as e:
                print(f"Real-ESRGAN upscaling failed: {e}")
                # If upscale fails but face restoration succeeded, return the GFPGAN result
                return {
                    "originalUrl": original_image_url,
                    "restoredUrl": restored_face_url,
                    "pipeline": pipeline_steps,
                    "status": "completed", # Completed up to GFPGAN
                    "error": f"Real-ESRGAN failed: {e}"
                }
        
        return {
            "originalUrl": original_image_url,
            "restoredUrl": upscaled_url if upscaled_url else restored_face_url, # Return upscaled if available, else gfpgan
            "pipeline": pipeline_steps,
            "status": "completed" if (restored_face_url or upscaled_url) else "failed"
        }

replicate_service = ReplicateService()
