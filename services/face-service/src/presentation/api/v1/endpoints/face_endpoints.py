from fastapi import APIRouter, File, UploadFile, HTTPException, Query, Body, Depends
from typing import List, Optional, Dict, Any
import uuid
import base64
import io
from PIL import Image
import logging

from src.domain.entities.models import BoundingBox, FaceDetectionResult, FaceMetadata, FaceSearchRequest, FaceSearchResult, FaceAddVectorRequest, FaceSearchVectorRequest
from src.application.services.face_manager import FaceManager
from src.presentation.dependencies import get_face_manager

router = APIRouter()
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

def _generate_thumbnail(image: Image.Image, bbox: List[int]) -> Optional[str]:
    """Helper to generate base64 encoded thumbnail from cropped face."""
    x, y, w, h = bbox
    cropped_face = image.crop((x, y, x + w, y + h))
    buffered = io.BytesIO()
    cropped_face.save(buffered, format="PNG")
    return base64.b64encode(buffered.getvalue()).decode("utf-8")

@router.post("/detect", response_model=List[FaceDetectionResult])
async def detect_faces(
    file: UploadFile = File(...),
    return_crop: Optional[bool] = Query(
        False,
        description="Whether to return base64 encoded cropped face images",
    ),
    face_manager: FaceManager = Depends(get_face_manager)
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
        image_data = await file.read()
        image = Image.open(io.BytesIO(image_data)).convert("RGB")

        detected_faces_with_embeddings = face_manager.detect_and_embed_faces(image)
        logger.info(f"Face manager returned {len(detected_faces_with_embeddings)} detections with embeddings.")
        logger.debug(f"Detections with embeddings: {detected_faces_with_embeddings}")

        if not detected_faces_with_embeddings:
            logger.info("No faces detected in the image.")
            raise HTTPException(
                status_code=404, detail="No faces detected in the image."
            )

        results: List[FaceDetectionResult] = []
        for det_with_embed in detected_faces_with_embeddings:
            x, y, w, h = det_with_embed["box"]
            
            face_result = FaceDetectionResult(
                id=str(uuid.uuid4()),
                bounding_box=BoundingBox(x=int(x), y=int(y), width=int(w), height=int(h)),
                confidence=float(det_with_embed["confidence"]),
                thumbnail=_generate_thumbnail(image, det_with_embed["box"]) if return_crop else None,
                embedding=det_with_embed["embedding"],
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


@router.post("/faces", response_model=Dict[str, Any])
async def add_face_with_metadata(
    file: UploadFile = File(...),
    metadata: str = Body(
        ..., description="JSON string of FaceMetadata"
    ),
    face_manager: FaceManager = Depends(get_face_manager),
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

        result = await face_manager.add_face(face_image, metadata_dict)
        logger.info(f"Face added successfully: {result['face_id']}")
        return result
    except Exception as e:
        logger.error(f"Failed to add face: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to add face: {e}")


@router.post("/faces/vector", response_model=Dict[str, Any])
async def add_face_by_vector(
    request: FaceAddVectorRequest,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(
        f"Received request to add face by vector for memberId: {request.metadata.member_id}"
    )
    try:
        metadata_dict = request.metadata.model_dump()
        result = await face_manager.add_face_by_vector(request.vector, metadata_dict)
        logger.info(f"Face added by vector successfully: {result['face_id']}")
        return result
    except Exception as e:
        logger.error(f"Failed to add face by vector: {e}", exc_info=True)
        raise HTTPException(
            status_code=500, detail=f"Failed to add face by vector: {e}"
        )


@router.post("/faces/search_by_vector", response_model=List[FaceSearchResult])
async def search_faces_by_vector(
    request: FaceSearchVectorRequest,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(
        f"Received request to search faces by vector. FamilyId: {request.family_id}, MemberId: {request.member_id}, TopK: {request.top_k}, Threshold: {request.threshold}"
    )
    try:
        search_results = await face_manager.search_similar_faces_by_vector(
            request.embedding, request.family_id, request.member_id, request.top_k, request.threshold
        )
        logger.info(f"Returning {len(search_results)} search results by vector.")
        return search_results
    except Exception as e:
        logger.error(f"Failed to search faces by vector: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to search faces by vector: {e}")


@router.get("/faces/family/{family_id}", response_model=List[Dict[str, Any]])
async def get_faces_by_family(
    family_id: str,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(f"Received request to get faces for family_id: {family_id}")
    try:
        faces = await face_manager.get_faces_by_family_id(family_id)
        logger.info(f"Returning {len(faces)} faces for family_id: {family_id}")
        return faces
    except Exception as e:
        logger.error(
            f"Failed to retrieve faces for family {family_id}: {e}", exc_info=True
        )
        raise HTTPException(status_code=500, detail=f"Failed to retrieve faces: {e}")


@router.delete("/faces/{face_id}", response_model=Dict[str, str])
async def delete_face_by_id(
    face_id: str,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(f"Received request to delete face with face_id: {face_id}")
    try:
        success = await face_manager.delete_face(face_id)
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


@router.delete("/faces/family/{family_id}", response_model=Dict[str, str])
async def delete_faces_by_family_id(
    family_id: str,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(f"Received request to delete faces for family_id: {family_id}")
    try:
        success = await face_manager.delete_faces_by_family_id(family_id)
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


@router.post("/faces/search", response_model=List[FaceSearchResult])
async def search_faces(
    request: FaceSearchRequest,
    face_manager: FaceManager = Depends(get_face_manager),
):
    logger.info(
        f"Received request to search faces. FamilyId: {request.family_id}, Limit: {request.limit}"
    )
    try:
        image_bytes = base64.b64decode(request.query_image)
        query_image = Image.open(io.BytesIO(image_bytes)).convert("RGB")

        search_results = await face_manager.search_similar_faces(
            query_image, request.family_id, request.limit
        )
        logger.info(f"Returning {len(search_results)} search results.")
        return search_results
    except Exception as e:
        logger.error(f"Failed to search faces: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Failed to search faces: {e}")
