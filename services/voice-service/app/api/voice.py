import httpx
from fastapi import APIRouter, HTTPException, Depends
from typing import List
from loguru import logger
from pydantic import BaseModel, Field

from app.services.preprocess_service import PreprocessService, get_preprocess_service
from app.services.replicate_service import ReplicateService, get_replicate_service
# No need for storage_service directly in generate, as Replicate returns URL


router = APIRouter(prefix="/voice", tags=["Voice"])


# --- Pydantic Models ---
class PreprocessRequest(BaseModel):
    audio_urls: List[str] = Field(..., description="List of URLs to audio files to be preprocessed.")


class PreprocessResponse(BaseModel):
    processed_audio_url: str = Field(..., description="URL of the combined and processed audio file.")
    duration: float = Field(..., description="Total duration of the processed audio file in seconds.")


class GenerateRequest(BaseModel):
    speaker_wav_url: str = Field(..., description="URL of the speaker's WAV audio for voice cloning.")
    text: str = Field(..., min_length=1, max_length=1000, description="Text to be converted to speech.")
    language: str = Field("vi", description="Language of the text (e.g., 'en', 'vi').")


class GenerateResponse(BaseModel):
    audio_url: str = Field(..., description="URL of the generated audio file.")


# --- API Endpoints ---
@router.post("/preprocess", response_model=PreprocessResponse, status_code=200)
async def preprocess_voice(
    request: PreprocessRequest,
    preprocess_service: PreprocessService = Depends(get_preprocess_service)
):
    """
    Tiền xử lý nhiều đoạn audio giọng nói thành 1 file WAV sạch.
    """
    logger.info(f"Received /preprocess request for {len(request.audio_urls)} audio URLs.")
    # logger.debug(f"Audio URLs: {request.audio_urls}")  # Avoid logging sensitive/long data

    try:
        processed_url, duration = await preprocess_service.process_audio_pipeline(request.audio_urls)
        logger.info(f"Successfully preprocessed audio. URL: {processed_url}, Duration: {duration:.2f}s")
        return PreprocessResponse(processed_audio_url=processed_url, duration=duration)
    except ValueError as e:
        logger.warning(f"Preprocessing failed due to bad request: {e}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Internal server error during preprocessing: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Internal server error during preprocessing: {e}")


@router.post("/generate", response_model=GenerateResponse, status_code=200)
async def generate_voice(
    request: GenerateRequest,
    replicate_service: ReplicateService = Depends(get_replicate_service)
):
    """
    Sinh audio giọng nói từ text dựa trên giọng mẫu đã preprocess.
    """
    logger.info(f"Received /generate request. Speaker WAV: {request.speaker_wav_url}, Language: {request.language}")
    # logger.debug(f"Text to generate: {request.text}")  # Avoid logging sensitive/long data

    # 1. Validate text
    if not request.text or len(request.text) > 1000:
        logger.warning("Invalid text for voice generation. Text cannot be empty or exceed 1000 characters.")
        raise HTTPException(
            status_code=400,
            detail="Text must not be empty and should not exceed 1000 characters."
        )

    try:
        audio_url = await replicate_service.generate_voice(
            request.speaker_wav_url,
            request.text,
            request.language
        )
        logger.info(f"Successfully generated voice. Audio URL: {audio_url}")
        return GenerateResponse(audio_url=audio_url)
    except ValueError as e:  # This is for Replicate API errors or custom service errors
        logger.error(f"Error from Replicate service during voice generation: {e}")
        # Task.md specifies 502 for Replicate timeout, 400 for other errors.
        # Here we catch general ValueError from replicate_service
        if "timeout" in str(e).lower() or "replicate api error" in str(e).lower():
            raise HTTPException(status_code=502, detail=f"Replicate service error: {e}")
        else:
            raise HTTPException(status_code=400, detail=f"Voice generation failed: {e}")
    except httpx.ConnectError as e:
        logger.error(f"Network connection error to Replicate during voice generation: {e}")
        raise HTTPException(status_code=502, detail=f"Network error connecting to Replicate: {e}")
    except Exception as e:
        logger.error(f"Internal server error during voice generation: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Internal server error during voice generation: {e}")
