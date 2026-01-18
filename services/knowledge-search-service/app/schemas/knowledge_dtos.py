from typing import Any
from pydantic import BaseModel


class GenericKnowledgeDto(BaseModel):
    # Metadata can contain any additional information about the knowledge,
    # such # as original IDs, types (family, member, event), visibility, etc.
    metadata: dict[str, Any]
    # Summary is a condensed text representation of the knowledge.
    summary: str


class KnowledgeIndexRequest(BaseModel):
    data: GenericKnowledgeDto
    action: str = "index"  # "index" or "delete"
