from fastapi import APIRouter, HTTPException, status, Depends, Request
from loguru import logger

from ..core.lancedb import KnowledgeLanceDBService
from ..core.embeddings import EmbeddingService, embedding_service
from ..schemas.vectors import VectorData, DeleteVectorRequest
from ..schemas.knowledge_dtos import KnowledgeAddRequest


router = APIRouter()


# Dependencies
def get_knowledge_lancedb_service(request: Request) -> KnowledgeLanceDBService:
    return request.app.state.knowledge_lancedb_service


def get_embedding_service() -> EmbeddingService:
    return embedding_service


@router.post("/knowledge", status_code=status.HTTP_201_CREATED)
async def add_knowledge_data(
    request: KnowledgeAddRequest,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service),
    embedding_service_dep: EmbeddingService = Depends(
        get_embedding_service
    ),  # Renamed to avoid conflict
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
            detail=(
                "Missing essential metadata: family_id, content_type, or original_id."
            ),
        )

    table_name = lancedb_service._get_knowledge_table_name(family_id)

    vector_data = VectorData(
        family_id=family_id,
        entity_id=str(original_id),
        type=content_type,
        visibility=metadata.get("visibility", "public"),
        name=metadata.get("name", str(original_id)),
        summary=summary,
        metadata=metadata,
    )

    await lancedb_service.add_vectors(table_name, [vector_data])
    return {
        "message": (
            f"{content_type} data with original_id "
            f"{original_id} added successfully for family "
            f"{family_id}."
        )
    }


@router.post("/knowledge/upsert", status_code=status.HTTP_200_OK)
async def upsert_knowledge_data(
    request: KnowledgeAddRequest,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service),
    embedding_service_dep: EmbeddingService = Depends(get_embedding_service),
):
    """
    Upserts new knowledge data to LanceDB.
    If data with the same family_id, original_id, and content_type already exists, it will be overwritten.
    Otherwise, it will be added as new data.
    """
    logger.info(
        f"Received knowledge upsert request for family_id: "
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
            detail=(
                "Missing essential metadata: family_id, content_type, or original_id."
            ),
        )

    table_name = lancedb_service._get_knowledge_table_name(family_id)

    # First, delete any existing data with the same unique identifiers
    delete_where_clause = (
        f"family_id = '{family_id}' AND "
        f"entity_id = '{original_id}' AND "
        f"type = '{content_type}'"
    )
    delete_request = DeleteVectorRequest(
        family_id=family_id,
        entity_id=original_id,
        where_clause=delete_where_clause,
    )
    await lancedb_service.delete_vectors(table_name, delete_request)
    logger.info(
        f"Existing knowledge data (if any) deleted for family_id: {family_id}, "
        f"original_id: {original_id}, content_type: {content_type}"
    )

    # Then, add the new knowledge data
    vector_data = VectorData(
        family_id=family_id,
        entity_id=str(original_id),
        type=content_type,
        visibility=metadata.get("visibility", "public"),
        name=metadata.get("name", str(original_id)),
        summary=summary,
        metadata=metadata,
    )

    await lancedb_service.add_vectors(table_name, [vector_data])
    return {
        "message": (
            f"{content_type} data with original_id "
            f"{original_id} upserted successfully for family "
            f"{family_id}."
        )
    }


@router.delete("/knowledge/family-data/{family_id}", status_code=status.HTTP_200_OK)
async def delete_knowledge_by_family_id(
    family_id: str,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service),
):
    """
    Deletes all knowledge data for a given family_id by dropping the associated LanceDB table.
    """
    logger.info(f"Received request to delete all knowledge for family_id: {family_id}")
    try:
        await lancedb_service.delete_knowledge_by_family_id(family_id)
        return {"message": f"All knowledge data for family_id '{family_id}' deleted successfully."}
    except Exception as e:
        logger.exception(f"Error deleting knowledge by family_id '{family_id}': {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error deleting knowledge data for family_id '{family_id}': {e}",
        )


@router.delete("/knowledge/{family_id}/{original_id}", status_code=status.HTTP_200_OK)
async def delete_knowledge_data(
    family_id: str,
    original_id: str,
    lancedb_service: KnowledgeLanceDBService = Depends(get_knowledge_lancedb_service),
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
        entity_id=original_id,  # Use entity_id for deletion
        where_clause=None,  # Explicitly set to None as we're using entity_id
    )

    deleted_count = await lancedb_service.delete_vectors(table_name, delete_request)

    if deleted_count == 0:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"No knowledge found with original_id '{original_id}' "
            f"for family '{family_id}' to delete.",
        )

    return {
        "message": (
            f"Knowledge data with original_id '{original_id}' "
            f"deleted successfully for family '{family_id}'."
        )
    }
