from typing import List, Literal
from pydantic import BaseModel

class SearchRequest(BaseModel):
    family_id: str
    query: str
    top_k: int = 10
    allowed_visibility: List[Literal["public", "private"]]

class SearchResultItem(BaseModel):
    member_id: str
    name: str
    summary: str
    score: float

class SearchResponse(BaseModel):
    results: List[SearchResultItem]
