from typing import List, Literal, Any
from pydantic import BaseModel


class SearchRequest(BaseModel):
    family_id: str
    query: str
    top_k: int = 10
    allowed_visibility: List[Literal["public", "private"]]


class SearchResultItem(BaseModel):
    # To store original_id, family_id, content_type, and other specific details
    metadata: dict[str, Any]
    summary: str
    score: float


class SearchResponse(BaseModel):
    results: List[SearchResultItem]
