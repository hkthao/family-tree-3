from fastapi import APIRouter, HTTPException, status
from loguru import logger

from ..models.schemas import SearchRequest, SearchResponse, SearchResultItem
from ..core.embeddings import embedding_service  # Import the instance directly
from ..core.lancedb import lancedb_service


router = APIRouter()


@router.post("/", response_model=SearchResponse)
async def search_knowledge(request: SearchRequest):
    """
    Performs a vector search for knowledge within a specific family's LanceDB
    table.
    """
    try:
        logger.info(f"Received search request for family_id: "
                    f"{request.family_id}, query: '{request.query[:50]}...'")

        # 1. Embed the query
        query_vector = embedding_service.embed_query(request.query)

        # 2. Search LanceDB
        results = lancedb_service.search_family_table(
            family_id=request.family_id,
            query_vector=query_vector,
            allowed_visibility=request.allowed_visibility,
            top_k=request.top_k
        )

        # 3. Format results to SearchResultItem
        formatted_results = [
            SearchResultItem(
                metadata=item["metadata"],
                summary=item["summary"],
                score=item["score"]
            ) for item in results
        ]

        logger.info(f"Search for family_id {request.family_id} returned "
                    f"{len(formatted_results)} results.")
        return SearchResponse(results=formatted_results)

    except Exception as e:
        logger.error(f"Error during knowledge search: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="An internal server error occurred during search."
        )
