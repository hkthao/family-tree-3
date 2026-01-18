from typing import List, Dict, Any, Literal
from pydantic import BaseModel, Field

# Request Model
class EmbeddingRequest(BaseModel):
    model: str
    input: str
    encoding_format: Literal["float", "base64"] = "float" # Optional, default to float

# Response Models
class EmbeddingData(BaseModel):
    object: str = "embedding"
    embedding: List[float]
    index: int = 0

class EmbeddingUsage(BaseModel):
    prompt_tokens: int = 0
    total_tokens: int = 0

class EmbeddingResponse(BaseModel):
    object: str = "list"
    data: List[EmbeddingData]
    model: str
    usage: EmbeddingUsage = Field(default_factory=EmbeddingUsage)