from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field

# --- Nested Models ---

class BoundingBoxModel(BaseModel):
    X: float
    Y: float
    Width: float
    Height: float

class MetadataModel(BaseModel):
    FamilyId: str
    MemberId: str
    FaceId: str # Corresponds to MemberFace.Id in backend
    BoundingBox: BoundingBoxModel
    Confidence: float
    ThumbnailUrl: Optional[str] = None
    OriginalImageUrl: Optional[str] = None
    Emotion: Optional[str] = None
    EmotionConfidence: float = 0.0

class FaceAddRequestModel(BaseModel):
    Vector: List[float]
    Metadata: MetadataModel

# --- Main Message Models ---

class MemberFaceAddedMessage(BaseModel):
    FaceAddRequest: FaceAddRequestModel
    MemberFaceLocalId: str # Redundant with FaceAddRequest.Metadata.FaceId, but included as per description

class MemberFaceDeletedMessage(BaseModel):
    MemberFaceId: str # Local ID of MemberFace entity
    VectorDbId: Optional[str] = None # ID of the face in the external vector database (Qdrant faceId)
    MemberId: str
    FamilyId: str

# --- Constants ---
class MessageBusConstants:
    class Exchanges:
        MEMBER_FACE = "member-face-exchange"

    class RoutingKeys:
        MEMBER_FACE_ADDED = "member-face.added"
        MEMBER_FACE_DELETED = "member-face.deleted"
