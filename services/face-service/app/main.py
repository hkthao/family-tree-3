from fastapi import FastAPI, File, UploadFile, HTTPException, Query, Form # Added Form
from fastapi.responses import JSONResponse, Response # Added Response
from typing import List, Optional, Tuple # Added Tuple
import uuid
import base64
import io
import json # Added
from PIL import Image
import numpy as np
import cv2 # Added

from pydantic import BaseModel # Added for DTOs

from app.services.face_detector import DlibFaceDetector
from app.services.face_embedding import FaceEmbeddingService


from app.services.get_emotion import get_emotion
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format=(
        '%(asctime)s - %(levelname)s - %(message)s'
    )
)
logger = logging.getLogger(__name__)

# Models for face detection results (redefined here from app.models.face_detection)
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
    emotion: Optional[str] = None
    emotion_confidence: Optional[float] = None

# New DTOs for image-processing integration





app = FastAPI(
    title="ImageFaceEmotionService", # Changed title
    description=(
        "A FastAPI service for face detection, embedding, cropping, and emotion analysis." # Changed description
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

            # Encode cropped face to base64 for emotion detection
            buffered_emotion = io.BytesIO()
            cropped_face.save(buffered_emotion, format="PNG")
            cropped_face_base64_emotion = base64.b64encode(buffered_emotion.getvalue()).decode("utf-8")

            # Perform emotion analysis
            predicted_emotion, emotion_confidence = get_emotion(cropped_face_base64_emotion)
            
            # Generate embedding for the cropped face
            face_embedding = face_embedding_service.get_facenet_embedding(
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
                emotion=predicted_emotion,
                emotion_confidence=emotion_confidence,
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





if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)  # noqa:E501
