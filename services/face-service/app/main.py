from fastapi import FastAPI, File, UploadFile, HTTPException, Query, Body
from typing import List, Optional, Dict, Any
import uuid
import base64
import io
from PIL import Image
import numpy as np

import asyncio
from app.message_consumer import MessageConsumer
from app.models import BoundingBox, FaceDetectionResult, FaceMetadata, FaceSearchRequest, FaceSearchResult, FaceAddVectorRequest, FaceSearchVectorRequest
from contextlib import asynccontextmanager

from app.services.face_detector import DlibFaceDetector
from app.services.face_embedding import FaceEmbeddingService
from app.services.qdrant_service import QdrantService
from app.services.face_service import FaceService
import uvicorn

import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO, format=("%(asctime)s - %(levelname)s - %(message)s")
)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting up application and message consumer...")
    asyncio.create_task(message_consumer.start())  # Run consumer in a background task
    yield
    logger.info("Shutting down application and message consumer...")
    await message_consumer.stop()  # Stop consumer gracefully


app = FastAPI(
    title="ImageFaceService",  # Changed title
    description=(
        "A FastAPI service for face detection, embedding, and cropping. "
        "Also provides face management and search using Qdrant."  # Updated description
    ),
    version="1.0.0",
    lifespan=lifespan,
)

# Initialize the face detector and embedding service
face_detector = DlibFaceDetector()

# Initialize Qdrant Service
qdrant_service = QdrantService()

# Initialize Face Embedding Service
face_embedding_service = FaceEmbeddingService()

# Initialize Face Service
face_service = FaceService(
    qdrant_service=qdrant_service, face_embedding_service=face_embedding_service
)

# Initialize Message Consumer
message_consumer = MessageConsumer(face_service=face_service)


@app.post("/detect", response_model=List[FaceDetectionResult])
async def detect_faces(
    file: UploadFile = File(...),
    return_crop: Optional[bool] = Query(
        False,
        description="Whether to return base64 encoded cropped face images",
    ),
):
    logger.info(
        "Received request to detect faces. Filename: %s, ReturnCrop: %s",
        file.filename,
        return_crop,
    )

    if not file.content_type.startswith("image/"):
        logger.warning(f"Invalid file type received: {file.content_type}")
        raise HTTPException(
            status_code=400, detail="Invalid file type. Only images are allowed."
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
                status_code=404, detail="No faces detected in the image."
            )

        results: List[FaceDetectionResult] = []
        for det in detections:
            x, y, w, h = det["box"]
            confidence = det["confidence"]

            face_id = str(uuid.uuid4())
            bounding_box = BoundingBox(x=int(x), y=int(y), width=int(w), height=int(h))

            thumbnail_base64 = None
            face_embedding = None

            # Crop face
            cropped_face = image.crop((x, y, x + w, y + h))

            # Generate embedding for the cropped face
            face_embedding = face_embedding_service.get_embedding(cropped_face)

            if return_crop:
                buffered = io.BytesIO()
                cropped_face.save(buffered, format="PNG")
                thumbnail_base64 = base64.b64encode(buffered.getvalue()).decode("utf-8")

            face_result = FaceDetectionResult(
                id=face_id,
                bounding_box=bounding_box,
                confidence=float(confidence),
                thumbnail=thumbnail_base64,
                embedding=face_embedding,
            )
            results.append(face_result)
            logger.debug(
                "Generated FaceDetectionResult: %s", face_result.model_dump_json()
            )

        logger.info(f"Returning {len(results)} face detection results.")
        return results

    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(f"Face detection failed: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Face detection failed: {e}")


@app.post("/faces", response_model=Dict[str, Any])
async def add_face_with_metadata(
    file: UploadFile = File(...),
    metadata: str = Body(
        ..., description="JSON string of FaceMetadata"
    ),  # Metadata as JSON string
):
    logger.info(
        "Received request to add face with metadata. Filename: %s", file.filename
    )
    if not file.content_type.startswith("image/"):
        logger.warning(f"Invalid file type received: {file.content_type}")
        raise HTTPException(
            status_code=400, detail="Invalid file type. Only images are allowed."
        )

    try:
        image_data = await file.read()
        face_image = Image.open(io.BytesIO(image_data)).convert("RGB")

        metadata_dict = FaceMetadata.model_validate_json(metadata).model_dump()

        result = face_service.add_face(face_image, metadata_dict)
        logger.info(f"Face added successfully: {result['face_id']}")
        return result
    except Exception as e:
        logger.error(f"Failed to add face: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to add face: {e}")


@app.post("/faces/vector", response_model=Dict[str, Any])
async def add_face_by_vector(request: FaceAddVectorRequest):
    logger.info(
        f"Received request to add face by vector for memberId: {request.metadata.member_id}"
    )
    try:
        metadata_dict = request.metadata.model_dump()
        result = face_service.add_face_by_vector(request.vector, metadata_dict)
        logger.info(f"Face added by vector successfully: {result['face_id']}")
        return result
    except Exception as e:
        logger.error(f"Failed to add face by vector: {e}", exc_info=True)
        raise HTTPException(
            status_code=500, detail=f"Failed to add face by vector: {e}"
        )


@app.post("/faces/search_by_vector", response_model=List[FaceSearchResult])
async def search_faces_by_vector(request: FaceSearchVectorRequest):
    logger.info(
        f"Received request to search faces by vector. FamilyId: {request.family_id}, MemberId: {request.member_id}, TopK: {request.top_k}, Threshold: {request.threshold}"
    )
    try:
        search_results = face_service.search_similar_faces_by_vector(
            request.embedding, request.family_id, request.member_id, request.top_k, request.threshold
        )
        logger.info(f"Returning {len(search_results)} search results by vector.")
        return search_results
    except Exception as e:
        logger.error(f"Failed to search faces by vector: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to search faces by vector: {e}")


@app.get("/faces/family/{family_id}", response_model=List[Dict[str, Any]])
async def get_faces_by_family(family_id: str):
    logger.info(f"Received request to get faces for family_id: {family_id}")
    try:
        faces = face_service.get_faces_by_family_id(family_id)
        logger.info(f"Returning {len(faces)} faces for family_id: {family_id}")
        return faces
    except Exception as e:
        logger.error(
            f"Failed to retrieve faces for family {family_id}: {e}", exc_info=True
        )
        raise HTTPException(status_code=500, detail=f"Failed to retrieve faces: {e}")


@app.delete("/faces/{face_id}", response_model=Dict[str, str])
async def delete_face_by_id(face_id: str):
    logger.info(f"Received request to delete face with face_id: {face_id}")
    try:
        success = face_service.delete_face(face_id)
        if success:
            logger.info(f"Face {face_id} deleted successfully.")
            return {"message": f"Face {face_id} deleted successfully."}
        else:
            raise HTTPException(
                status_code=404,
                detail=f"Face with ID {face_id} not found or could not be deleted.",
            )
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(f"Failed to delete face {face_id}: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to delete face: {e}")


@app.delete("/faces/family/{family_id}", response_model=Dict[str, str])
async def delete_faces_by_family_id(family_id: str):
    logger.info(f"Received request to delete faces for family_id: {family_id}")
    try:
        success = face_service.delete_faces_by_family_id(family_id)
        if success:
            logger.info(f"Faces for family {family_id} deleted successfully.")
            return {"message": f"Faces for family {family_id} deleted successfully."}
        else:
            raise HTTPException(
                status_code=404,
                detail=f"No faces found for family with ID {family_id} or could not be deleted.",
            )
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(
            f"Failed to delete faces for family {family_id}: {e}", exc_info=True
        )
        raise HTTPException(
            status_code=500, detail=f"Failed to delete faces for family: {e}"
        )


@app.post("/faces/search", response_model=List[FaceSearchResult])
async def search_faces(request: FaceSearchRequest):
    logger.info(
        f"Received request to search faces. FamilyId: {request.family_id}, Limit: {request.limit}"
    )
    try:
        # Decode the base64 image
        image_bytes = base64.b64decode(request.query_image)
        query_image = Image.open(io.BytesIO(image_bytes)).convert("RGB")

        search_results = face_service.search_similar_faces(
            query_image, request.family_id, request.limit
        )
        logger.info(f"Returning {len(search_results)} search results.")
        return search_results
    except Exception as e:
        logger.error(f"Failed to search faces: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to search faces: {e}")

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
