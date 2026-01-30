from typing import List, Optional, Dict, Any
from pydantic import BaseModel


# --- Nested Models ---

class BoundingBoxModel(BaseModel):
    x: float
    y: float
    width: float
    height: float


class MetadataModel(BaseModel):
    family_id: str
    member_id: str
    face_id: str  # Corresponds to MemberFace.Id in backend
    bounding_box: BoundingBoxModel
    confidence: float
    thumbnail_url: Optional[str] = None
    original_image_url: Optional[str] = None
    emotion: Optional[str] = None
    emotion_confidence: float = 0.0


class FaceAddRequestModel(BaseModel):
    vector: List[float]
    metadata: MetadataModel


# --- Main Message Models ---

class MemberFaceAddedMessage(BaseModel):
    face_add_request: FaceAddRequestModel
    member_face_local_id: str  # Redundant with FaceAddRequest.Metadata.face_id, but included as per description


class MemberFaceDeletedMessage(BaseModel):
    member_face_id: str
    vector_db_id: Optional[str] = None
    member_id: str
    family_id: str


# --- Constants ---
class MessageBusConstants:
    class Exchanges:
        MEMBER_FACE = "face_exchange"

    class RoutingKeys:
        MEMBER_FACE_ADDED = "face.add"
        MEMBER_FACE_DELETED = "face.delete"


# --- API Models ---

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


class FaceMetadata(BaseModel):
    face_id: str
    local_db_id: str
    member_id: str
    family_id: str
    thumbnail_url: str
    original_image_url: str
    emotion: str = ""
    emotion_confidence: float = 0.0


class FaceSearchRequest(BaseModel):
    query_image: str  # base64 encoded image
    family_id: Optional[str] = None
    limit: int = 5


class FaceSearchResult(BaseModel):
    id: str
    score: float
    payload: Dict[str, Any]


class FaceAddVectorRequest(BaseModel):
    vector: List[float]
    metadata: FaceMetadata


class FaceSearchVectorRequest(BaseModel):
    embedding: List[float]
    family_id: Optional[str] = None
    member_id: Optional[str] = None
    top_k: int = 5
    threshold: float = 0.7  # Add threshold for vector search

class BatchFaceSearchVectorRequest(BaseModel):
    vectors: List[List[float]]
    family_id: Optional[str] = None
    top_k: int = 1 # Changed from 5 to 1 based on the context of DetectFacesCommandHandler
    threshold: float = 0.7  # Add threshold for vector search
