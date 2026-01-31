from fastapi import APIRouter, HTTPException, status, Depends, Request
from loguru import logger

from ..models.schemas import SearchRequest, SearchResponse, SearchResultItem
from ..core.embeddings import EmbeddingService, embedding_service
from ..core.qdrant import KnowledgeQdrantService


router = APIRouter()


# Dependencies
def get_knowledge_qdrant_service(request: Request) -> KnowledgeQdrantService:
    return request.app.state.knowledge_qdrant_service


def get_embedding_service() -> EmbeddingService:
    return embedding_service


@router.post("/search", response_model=SearchResponse)
async def search_knowledge(
    request: SearchRequest,
    qdrant_service: KnowledgeQdrantService = Depends(get_knowledge_qdrant_service),
    embedding_service_dep: EmbeddingService = Depends(get_embedding_service)
):
    """
    Performs a vector search for knowledge within a specific family's LanceDB
    table.
    """
    try:
        logger.info(f"Received search request for family_id: "
                    f"{request.family_id}, query: '{request.query[:50]}...'")

        # 1. Embed the query
        query_vector = embedding_service_dep.embed_query(request.query)

        # 2. Search LanceDB
        results = await qdrant_service.search_knowledge_table(
            family_id=request.family_id,
            query_vector=query_vector,
            allowed_visibility=request.allowed_visibility,
            top_k=request.top_k
        )

        # 3. Format results to SearchResultItem
        formatted_results = [
            SearchResultItem(
                metadata=item.get("metadata", {}),
                summary=item.get("summary"),
                score=item.get("score", 0.0)
            ) for item in results
        ]
        logger.info(f"Search for family_id {request.family_id} returned "
                    f"{len(formatted_results)} results.")
        return SearchResponse(results=formatted_results)

    except Exception as e:
        logger.error("Error during knowledge search: {}", e, exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="An internal server error occurred during search."
        )
