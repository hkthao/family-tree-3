from fastapi import FastAPI, File, UploadFile, HTTPException, Query, Response
from typing import List, Optional
import uuid
import base64
import io
from PIL import Image
import numpy as np

from pydantic import BaseModel # Added for DTOs

from app.services.face_detector import DlibFaceDetector
from app.services.face_embedding import FaceEmbeddingService



import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format=(
        '%(asctime)s - %(levelname)s - %(message)s'
    )
)
logger = logging.getLogger(__name__)


class BoundingBox(BaseModel):
    x: int
    y: int
    width: int
    height: int

class FaceDetectionResult(BaseModel):
    id: str
    bounding_box: BoundingBox
    confidence: float
    thumbnail: Optional[str] = None
    embedding: Optional[List[float]] = None


# New DTOs for image-processing integration

app = FastAPI(
    title="ImageFaceService", # Changed title
    description=(
        "A FastAPI service for face detection, embedding, and cropping." # Changed description
    ),
    version="1.0.0",
)

# Initialize the face detector and embedding service
face_detector = DlibFaceDetector()
face_embedding_service = FaceEmbeddingService()


@app.post("/detect", response_model=List[FaceDetectionResult])
async def detect_faces(
    file: UploadFile = File(...),
    return_crop: Optional[bool] = Query(
        False,
        description="Whether to return base64 encoded cropped face images",
    ),
):
    logger.info("Received request to detect faces. Filename: %s, ReturnCrop: %s", file.filename, return_crop)

    if not file.content_type.startswith("image/"):
        logger.warning(f"Invalid file type received: {file.content_type}")
        raise HTTPException(
            status_code=400,
            detail="Invalid file type. Only images are allowed."
        )

    try:
        # Read image file
        image_data = await file.read()
        image = Image.open(io.BytesIO(image_data)).convert("RGB")
        image_np = np.array(image)

        # Perform face detection
        detections = face_detector.detect_faces(image_np)
        logger.info(f"Face detector returned {len(detections)} detections.")
        logger.debug(f"Detections: {detections}")  # Use debug for detailed output

        if not detections:
            logger.info("No faces detected in the image.")
            raise HTTPException(
                status_code=404,
                detail="No faces detected in the image."
            )

        results: List[FaceDetectionResult] = []
        for det in detections:
            x, y, w, h = det['box']
            confidence = det['confidence']

            face_id = str(uuid.uuid4())
            bounding_box = BoundingBox(
                x=int(x), y=int(y), width=int(w), height=int(h)
            )

            thumbnail_base64 = None
            face_embedding = None

            # Crop face
            cropped_face = image.crop((x, y, x + w, y + h))
            
            # Generate embedding for the cropped face
            face_embedding = face_embedding_service.get_embedding( # Changed from get_facenet_embedding
                cropped_face
            )

            if return_crop:
                buffered = io.BytesIO()
                cropped_face.save(buffered, format="PNG")
                thumbnail_base64 = base64.b64encode(buffered.getvalue()).decode(
                    "utf-8"
                )

            face_result = FaceDetectionResult(
                id=face_id,
                bounding_box=bounding_box,
                confidence=float(confidence),
                thumbnail=thumbnail_base64,
                embedding=face_embedding,
            )
            results.append(face_result)
            logger.debug(
                "Generated FaceDetectionResult: %s",
                face_result.json()  # noqa: E501
            )

        logger.info(f"Returning {len(results)} face detection results.")
        return results

    except Exception as e:
        logger.error(f"Face detection failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"Face detection failed: {e}"
        )


@app.post("/resize")
async def resize_image(
    file: UploadFile = File(...),
    width: int = Query(..., description="Desired width for the resized image"),
    height: Optional[int] = Query(None, description="Desired height for the resized image (optional, aspect ratio will be maintained if not provided)"),
):
    logger.info("Received request to resize image. Filename: %s, Target Width: %s, Target Height: %s", file.filename, width, height)

    if not file.content_type.startswith("image/"):
        logger.warning(f"Invalid file type received for resize: {file.content_type}")
        raise HTTPException(
            status_code=400,
            detail="Invalid file type. Only images are allowed."
        )

    try:
        image_data = await file.read()
        image = Image.open(io.BytesIO(image_data))

        original_width, original_height = image.size

        if height is None:
            # Maintain aspect ratio
            aspect_ratio = original_height / original_width
            new_height = int(width * aspect_ratio)
            size = (width, new_height)
            logger.info(f"Resizing image to {size[0]}x{size[1]} (maintaining aspect ratio).")
            image.thumbnail(size, Image.LANCZOS) # Use thumbnail to maintain aspect ratio and not upscale
        else:
            size = (width, height)
            logger.info(f"Resizing image to exact dimensions {size[0]}x{size[1]}.")
            image = image.resize(size, Image.LANCZOS) # Use resize for exact dimensions

        buffered = io.BytesIO()
        image.save(buffered, format=image.format if image.format else "PNG") # Preserve original format if possible
        buffered.seek(0)

        logger.info(f"Successfully resized image to {image.size[0]}x{image.size[1]}.")
        return Response(content=buffered.getvalue(), media_type=file.content_type)

    except Exception as e:
        logger.error(f"Image resize failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"Image resize failed: {e}"
        )


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)  # noqa:E501
