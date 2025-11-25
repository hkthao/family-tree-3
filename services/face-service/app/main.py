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
# Import the newly copied files
from .crop_face import crop_and_resize_face, encode_image_to_base64
from .get_emotion import get_emotion

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

# New DTOs for image-processing integration
class FaceLocation(BaseModel):
    top: int
    right: int
    bottom: int
    left: int

class CropAndAnalyzeFaceResponse(BaseModel):
    cropped_face_base64: str
    emotion: str
    confidence: float
    member_id: Optional[str] = None


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

@app.post("/crop-and-analyze-face/", response_model=CropAndAnalyzeFaceResponse)
async def crop_and_analyze_face_endpoint(
    file: UploadFile = File(...),
    face_location_json: str = Form(..., description="JSON string of face location (top, right, bottom, left)"),
    member_id: Optional[str] = Form(None, description="Optional member ID to associate with the face")
):
    """
    Crops a specific face from an uploaded image based on the provided location,
    resizes it to 512x512, and performs preliminary emotion analysis.
    Returns the base64 encoded cropped face and detected emotion.
    """
    logger.info(f"Received request to crop and analyze face for file: {file.filename}")
    try:
        image_bytes = await file.read()
        face_location_data = json.loads(face_location_json)
        
        # Convert dict to FaceLocation Pydantic model
        face_location = FaceLocation(**face_location_data)

        # Crop and resize the face
        # Note: crop_and_resize_face expects a tuple (top, right, bottom, left)
        # So convert from Pydantic model to tuple
        cropped_face_base64 = crop_and_resize_face(
            image_bytes, 
            (face_location.top, face_location.right, face_location.bottom, face_location.left), 
            target_size=512
        )
        if not cropped_face_base64:
            raise HTTPException(status_code=400, detail="Failed to crop and resize face.")

        # Perform preliminary emotion analysis
        predicted_emotion, confidence = get_emotion(cropped_face_base64)

        return CropAndAnalyzeFaceResponse(
            cropped_face_base64=cropped_face_base64,
            emotion=predicted_emotion,
            confidence=confidence,
            member_id=member_id,
        )
    except json.JSONDecodeError:
        logger.error(f"Invalid JSON format for face_location: {face_location_json}", exc_info=True)
        raise HTTPException(status_code=400, detail="Invalid JSON format for face_location.")
    except Exception as e:
        logger.error(f"Error processing face: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Error processing face: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)  # noqa:E501
