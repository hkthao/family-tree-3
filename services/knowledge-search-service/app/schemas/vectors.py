from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field


class VectorData(BaseModel):
    # These fields are explicit for LanceDB's schema and filtering/indexing
    family_id: str = Field(
        ..., description="The ID of the family the entity belongs to."
    )
    entity_id: str = Field(
        ...,
        description=(
            "The ID of the entity (member, event, or family). Must be unique "
            "within a family."
        ),
    )
    type: str = Field(
        ...,
        description=(
            "The type of the entity (e.g., 'family', 'member', 'event')."
        ),
    )
    visibility: str = Field(
        "public",
        description=(
            "The visibility of the entity (e.g., 'public', 'private', "
            "'deleted')."
        ),
    )
    name: str = Field(
        ..., description="The primary name or title of the entity."
    )
    summary: str = Field(
        ...,
        description=(
            "A summary or description of the entity. Used to generate "
            "embedding vector."
        ),
    )

    # This field stores the full, original metadata from the source system
    # (e.g., C# backend)
    # It allows storing arbitrary additional key-value pairs without modifying
    # the VectorData schema for every new detail.
    metadata: Dict[str, Any] = Field(
        default_factory=dict,
        description="Additional metadata associated with the entity.",
    )


class AddVectorRequest(BaseModel):
    vectors: List[VectorData] = Field(
        ..., description="List of vector data entries to add."
    )


class UpdateVectorRequest(BaseModel):
    family_id: str = Field(
        ..., description="The ID of the family the entity belongs to."
    )
    entity_id: str = Field(
        ..., description="The ID of the entity (member or family)."
    )
    # Fields to update - all optional
    type: Optional[str] = Field(None, description="New type of the entity.")
    visibility: Optional[str] = Field(
        None, description="New visibility of the entity."
    )
    name: Optional[str] = Field(None, description="New name of the entity.")
    summary: Optional[str] = Field(
        None, description="New summary of the entity."
    )
    # Allow updating the metadata dictionary
    metadata: Optional[Dict[str, Any]] = Field(
        None, description="New additional metadata associated with the entity."
    )


class DeleteVectorRequest(BaseModel):
    family_id: str = Field(
        ..., description="The ID of the family the entity belongs to."
    )
    # Either entity_id or where_clause should be provided for deletion
    entity_id: Optional[str] = Field(
        None,
        description=(
            "The ID of the entity to delete. If None, deletes all entities of "
            "specified type within the family."
        ),
    )
    type: Optional[str] = Field(
        None,
        description=(
            "The type of the entity to delete. Required if entity_id is None "
            "and family_id is specified."
        ),
    )
    where_clause: Optional[str] = Field(
        None,
        description=(
            "A SQL-like WHERE clause to filter vectors for deletion. E.g., "
            "'id = \"some_id\" AND content_type = \"Member\"'."
        ),
    )


class RebuildVectorRequest(BaseModel):
    family_id: str = Field(
        ..., description="The ID of the family to rebuild vectors for."
    )
    entity_ids: Optional[List[str]] = Field(
        None,
        description=(
            "Optional: List of specific entity IDs to rebuild. If None, "
            "rebuilds all vectors for the given family_id."
        ),
    )
