from fastapi import APIRouter, HTTPException, status, Depends
from loguru import logger
from typing import List, Optional
from uuid import UUID

from ..core.lancedb import KnowledgeLanceDBService, knowledge_lancedb_service
from ..core.embeddings import EmbeddingService, embedding_service
from ..schemas.vectors import VectorData, DeleteVectorRequest
from ..schemas.knowledge_dtos import KnowledgeAddRequest


router = APIRouter()

# Dependencies
def get_knowledge_lancedb_service() -> KnowledgeLanceDBService:
    return knowledge_lancedb_service

def get_embedding_service() -> EmbeddingService:
    return embedding_service


@router.post("/knowledge", status_code=status.HTTP_201_CREATED)
async def add_knowledge_data(
    request: KnowledgeAddRequest,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service),
    embedding_service_dep: EmbeddingService = Depends(get_embedding_service) # Renamed to avoid conflict
):
    """
    Adds new knowledge data to LanceDB.
    The request data should contain metadata and a summary.
    """
    logger.info(
        f"Received knowledge add request for family_id: "
        f"{request.data.metadata.get('family_id')}, "
        f"content_type: {request.data.metadata.get('content_type')}"
    )

    summary = request.data.summary
    metadata = request.data.metadata

    family_id = metadata.get("family_id")
    content_type = metadata.get("content_type")
    original_id = metadata.get("original_id")

    if not all([family_id, content_type, original_id]):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=("Missing essential metadata: family_id, content_type, "
                    "or original_id."),
        )

    table_name = lancedb_service._get_knowledge_table_name(family_id)

    vector_data = VectorData(
        family_id=family_id,
        entity_id=str(original_id),
        type=content_type,
        visibility=metadata.get("visibility", "public"),
        name=metadata.get("name", str(original_id)),
        summary=summary,
        metadata=metadata
    )

    lancedb_service.add_vectors(table_name, [vector_data])
    return {"message": (f"{content_type} data with original_id "
                        f"{original_id} added successfully for family "
                        f"{family_id}.")}


@router.delete("/knowledge/{family_id}/{original_id}", status_code=status.HTTP_200_OK)
async def delete_knowledge_data(
    family_id: str,
    original_id: str,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service)
):
    """
    Deletes knowledge data from LanceDB based on family_id and original_id.
    """
    logger.info(
        f"Received knowledge delete request for family_id: {family_id}, "
        f"original_id: {original_id}"
    )

    table_name = lancedb_service._get_knowledge_table_name(family_id)
    delete_request = DeleteVectorRequest(
        family_id=family_id,
        entity_id=original_id, # Use entity_id for deletion
        where_clause=None # Explicitly set to None as we're using entity_id
    )
    
    deleted_count = lancedb_service.delete_vectors(table_name, delete_request)

    if deleted_count == 0:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"No knowledge found with original_id '{original_id}' "
                   f"for family '{family_id}' to delete."
        )

    return {"message": (f"Knowledge data with original_id '{original_id}' "
                        f"deleted successfully for family '{family_id}'.")}
