from pydantic import BaseModel
from typing import List, Dict, Optional

class BoundingBox(BaseModel):
    x: int
    y: int
    width: int
    height: int

class FaceDetectionResult(BaseModel):
    id: str
    bounding_box: BoundingBox
    confidence: float
    thumbnail: Optional[str] = None  # Base64 encoded image

class Face(BaseModel):
    id: str
    bounding_box: BoundingBox
    confidence: float
    thumbnail: Optional[str] = None
