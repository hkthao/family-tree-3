from fastapi import APIRouter, HTTPException, status
from loguru import logger

from ..core.lancedb import lancedb_service
from ..core.embeddings import embedding_service
from ..schemas.vectors import VectorData, DeleteVectorRequest
from ..schemas.knowledge_dtos import KnowledgeIndexRequest

router = APIRouter()


@router.post("/index", status_code=status.HTTP_200_OK)
async def index_knowledge_data(
    request: KnowledgeIndexRequest,
):
    """
    Indexes or deletes knowledge data based on the provided request.
    The request data should contain metadata and a summary.
    """
    # Log relevant metadata for traceability
    logger.info(
        f"Received knowledge index request for family_id: "
        f"{request.data.metadata.get('family_id')}, "
        f"content_type: {request.data.metadata.get('content_type')}, "
        f"action: {request.action}"
    )
    return await _process_knowledge_request(request)


async def _process_knowledge_request(
    request: KnowledgeIndexRequest,
):
    # Extract necessary information from the new GenericKnowledgeDto structure
    summary = request.data.summary
    metadata = request.data.metadata

    # Ensure essential metadata fields are present
    family_id = metadata.get("family_id")
    content_type = metadata.get("content_type")
    # Assuming 'original_id' is the unique identifier for the item
    original_id = metadata.get("original_id")

    if not all([family_id, content_type, original_id]):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=("Missing essential metadata: family_id, content_type, "
                    "or original_id."),
        )

    table_name = lancedb_service._get_table_name(family_id)

    if request.action == "index":
        # The metadata dictionary already contains all the necessary
        # original fields
        vector_data = VectorData(
            family_id=family_id,
            entity_id=str(original_id),
            type=content_type,
            # Default to public if not provided in metadata
            visibility=metadata.get("visibility", "public"),
            # Default name to original_id if not provided
            name=metadata.get("name", str(original_id)),
            summary=summary,
            metadata=metadata  # Pass the entire metadata dictionary
        )

        lancedb_service.add_vectors(table_name, [vector_data])
        return {"message": (f"{content_type} data with original_id "
                            f"{original_id} indexed successfully for family "
                            f"{family_id}.")}

    elif request.action == "delete":
        delete_request = DeleteVectorRequest(
            family_id=family_id,
            # Use original_id for deletion
            # Assuming original_id is stored in metadata and indexed
            where_clause=f"original_id = '{original_id}'"
        )
        lancedb_service.delete_vectors(table_name, delete_request)
        return {"message": (f"{content_type} data with original_id "
                            f"{original_id} deleted successfully for family "
                            f"{family_id}.")}
    else:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid action specified."
        )
