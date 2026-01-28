from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field, ConfigDict


# --- Nested Models ---

class BoundingBoxModel(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    x: float = Field(alias="X")
    y: float = Field(alias="Y")
    width: float = Field(alias="Width")
    height: float = Field(alias="Height")


class MetadataModel(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    family_id: str = Field(alias="FamilyId")
    member_id: str = Field(alias="MemberId")
    face_id: str = Field(alias="FaceId")  # Corresponds to MemberFace.Id in backend
    bounding_box: BoundingBoxModel = Field(alias="BoundingBox")
    confidence: float = Field(alias="Confidence")
    thumbnail_url: Optional[str] = Field(alias="ThumbnailUrl", default=None)
    original_image_url: Optional[str] = Field(alias="OriginalImageUrl", default=None)
    emotion: Optional[str] = Field(alias="Emotion", default=None)
    emotion_confidence: float = Field(alias="EmotionConfidence", default=0.0)


class FaceAddRequestModel(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    vector: List[float] = Field(alias="Vector")
    metadata: MetadataModel = Field(alias="Metadata")


# --- Main Message Models ---

class MemberFaceAddedMessage(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    face_add_request: FaceAddRequestModel = Field(alias="FaceAddRequest")
    member_face_local_id: str = Field(alias="MemberFaceLocalId")  # Redundant with FaceAddRequest.Metadata.FaceId, but included as per description


class MemberFaceDeletedMessage(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    member_face_id: str = Field(alias="MemberFaceId")
    vector_db_id: Optional[str] = Field(alias="VectorDbId", default=None)
    member_id: str = Field(alias="MemberId")
    family_id: str = Field(alias="FamilyId")


# --- Constants ---
class MessageBusConstants:
    class Exchanges:
        MEMBER_FACE = "member-face-exchange"

    class RoutingKeys:
        MEMBER_FACE_ADDED = "member-face.added"
        MEMBER_FACE_DELETED = "member-face.deleted"


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
    faceId: str
    localDbId: str
    memberId: str
    familyId: str
    thumbnailUrl: str
    originalImageUrl: str
    emotion: str = ""
    emotionConfidence: float = 0.0


class FaceSearchRequest(BaseModel):
    query_image: str  # base64 encoded image
    familyId: Optional[str] = None
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
