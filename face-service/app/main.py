from fastapi import FastAPI, File, UploadFile, HTTPException, Query
from typing import List, Optional
import uuid
import base64
import io
from PIL import Image
import numpy as np

from app.services.face_detector import MTCNNFaceDetector
from app.models.face_detection import FaceDetectionResult, BoundingBox

import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI(
    title="FaceDetectionService",
    description="A FastAPI service for face detection using MTCNN.",
    version="1.0.0",
)

# Initialize the face detector
face_detector = MTCNNFaceDetector()

@app.post("/detect", response_model=List[FaceDetectionResult])
async def detect_faces(
    file: UploadFile = File(...),
    return_crop: Optional[bool] = Query(False, description="Whether to return base64 encoded cropped face images"),
):
    logger.info(f"Received request to detect faces. Filename: {file.filename}, ReturnCrop: {return_crop}")

    if not file.content_type.startswith("image/"):
        logger.warning(f"Invalid file type received: {file.content_type}")
        raise HTTPException(status_code=400, detail="Invalid file type. Only images are allowed.")

    try:
        # Read image file
        image_data = await file.read()
        image = Image.open(io.BytesIO(image_data)).convert("RGB")
        image_np = np.array(image)

        # Perform face detection
        detections = face_detector.detect_faces(image_np)
        logger.info(f"Face detector returned {len(detections)} detections.")
        logger.debug(f"Detections: {detections}") # Use debug for detailed output

        if not detections:
            logger.info("No faces detected in the image.")
            raise HTTPException(status_code=404, detail="No faces detected in the image.")

        results: List[FaceDetectionResult] = []
        for det in detections:
            x, y, w, h = det['box']
            confidence = det['confidence']

            face_id = str(uuid.uuid4())
            bounding_box = BoundingBox(x=int(x), y=int(y), width=int(w), height=int(h))

            thumbnail_base64 = None
            if return_crop:
                # Crop face
                cropped_face = image.crop((x, y, x + w, y + h))
                buffered = io.BytesIO()
                cropped_face.save(buffered, format="PNG")
                thumbnail_base64 = base64.b64encode(buffered.getvalue()).decode("utf-8")

            face_result = FaceDetectionResult(
                id=face_id,
                bounding_box=bounding_box,
                confidence=float(confidence),
                thumbnail=thumbnail_base64,
            )
            results.append(face_result)
            logger.debug(f"Generated FaceDetectionResult: {face_result.json()}") # Log each result

        logger.info(f"Returning {len(results)} face detection results.")
        return results

    except Exception as e:
        logger.error(f"Face detection failed: {e}", exc_info=True) # Log exception details
        raise HTTPException(status_code=500, detail=f"Face detection failed: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
