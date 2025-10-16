from fastapi import FastAPI, File, UploadFile, HTTPException, Query
from typing import List, Optional
import uuid
import base64
import io
from PIL import Image
import numpy as np

from app.services.face_detector import MTCNNFaceDetector
from app.models.face_detection import FaceDetectionResult, BoundingBox

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
    """
    Detects faces in an uploaded image.

    - **file**: The image file to upload.
    - **return_crop**: If True, returns base64 encoded cropped face images.
    """
    if not file.content_type.startswith("image/"):
        raise HTTPException(status_code=400, detail="Invalid file type. Only images are allowed.")

    try:
        # Read image file
        image_data = await file.read()
        image = Image.open(io.BytesIO(image_data)).convert("RGB")
        image_np = np.array(image)

        # Perform face detection
        detections = face_detector.detect_faces(image_np)

        if not detections:
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

            results.append(
                FaceDetectionResult(
                    id=face_id,
                    bounding_box=bounding_box,
                    confidence=float(confidence),
                    thumbnail=thumbnail_base64,
                )
            )
        return results

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Face detection failed: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
