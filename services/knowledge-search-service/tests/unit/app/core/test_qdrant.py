import pytest
from unittest.mock import MagicMock, AsyncMock, patch
from qdrant_client.http.models import UpdateStatus

import uuid

from qdrant_client import models
from qdrant_client.http.exceptions import UnexpectedResponse
from app.core.qdrant import KnowledgeQdrantService
from app.core.embeddings import EmbeddingService
from app.schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)
from app.config import TEXT_EMBEDDING_DIMENSIONS


@pytest.fixture
def mock_embedding_service():
    """Fixture for a mocked EmbeddingService."""
    service = MagicMock(spec=EmbeddingService)
    service.embed_query.return_value = [0.1] * TEXT_EMBEDDING_DIMENSIONS
    # Mock embed_documents to return a list of embeddings
    service.embed_documents.side_effect = lambda texts: [[0.1] * TEXT_EMBEDDING_DIMENSIONS for _ in texts]
    return service


@pytest.fixture
def mock_qdrant_client():
    """Fixture for a mocked QdrantClient."""
    client = MagicMock(spec_set=[
        'get_collection', 'create_collection', 'upsert', 'retrieve', 'delete', 'scroll', 'query_points', 'create_payload_index'
    ])
    # Mock async methods with AsyncMock
    client.get_collection = AsyncMock(return_value=MagicMock()) # Default to existing collection
    client.create_collection = AsyncMock()
    client.upsert = AsyncMock(return_value=MagicMock(status=UpdateStatus.COMPLETED))
    client.retrieve = AsyncMock(return_value=[MagicMock()]) # Default retrieve to return a list with a mock point
    client.delete = AsyncMock(return_value=MagicMock(status=UpdateStatus.COMPLETED, count=1))
    client.scroll = AsyncMock(return_value=([], None)) # Default empty scroll result
    client.query_points = AsyncMock(return_value=[]) # Default empty query_points result
    
    # Non-async methods (like create_payload_index) can remain MagicMock
    client.create_payload_index = AsyncMock()

    return client

@pytest.fixture
async def knowledge_qdrant_service(mock_embedding_service, mock_qdrant_client):
    """Fixture for KnowledgeQdrantService with mocked dependencies."""
    # Patch the AsyncQdrantClient constructor to return our mock_qdrant_client
    with patch('app.core.qdrant.AsyncQdrantClient', return_value=mock_qdrant_client):
        service = KnowledgeQdrantService(mock_embedding_service)
        # async_init is not called here, tests will call it explicitly
        return service


@pytest.mark.asyncio
async def test_create_collection_if_not_exists_new_collection(mock_embedding_service, mock_qdrant_client):
    mock_qdrant_client.get_collection.side_effect = UnexpectedResponse(status_code=404, reason_phrase="Not found", content=b"", headers=dict())
    with patch('app.core.qdrant.AsyncQdrantClient', return_value=mock_qdrant_client):
        service = KnowledgeQdrantService(mock_embedding_service)
        await service.async_init()
    mock_qdrant_client.create_collection.assert_called_once_with(
        collection_name=service.collection_name,
        vectors_config=models.VectorParams(
            size=TEXT_EMBEDDING_DIMENSIONS,
            distance=models.Distance.COSINE
        ),
    )
    assert mock_qdrant_client.create_payload_index.call_count == 4


@pytest.mark.asyncio
async def test_create_collection_if_not_exists_existing_collection(mock_embedding_service, mock_qdrant_client):
    mock_qdrant_client.get_collection.return_value = MagicMock() # Simulate collection exists
    with patch('app.core.qdrant.AsyncQdrantClient', return_value=mock_qdrant_client):
        service = KnowledgeQdrantService(mock_embedding_service)
        await service.async_init()
    mock_qdrant_client.create_collection.assert_not_called()
    assert mock_qdrant_client.create_payload_index.call_count == 4


@pytest.mark.asyncio
async def test_add_vectors_success(knowledge_qdrant_service, mock_embedding_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    entity_id = str(uuid.uuid4())
    vectors_data = [
        VectorData(
            family_id=family_id,
            entity_id=entity_id,
            type="member",
            visibility="public",
            name="Test Member",
            summary="This is a test summary.",
            metadata={"age": 30}
        )
    ]

    await knowledge_qdrant_service.add_vectors(vectors_data)

    mock_embedding_service.embed_documents.assert_called_once_with(["This is a test summary."])
    mock_qdrant_client.upsert.assert_called_once()
    args, kwargs = mock_qdrant_client.upsert.call_args
    points = args[0]['points'] if len(args) > 0 and 'points' in args[0] else kwargs['points']
    assert len(points) == 1
    point = points[0]
    assert point.id == f"{family_id}-{entity_id}"
    assert point.vector == [0.1] * TEXT_EMBEDDING_DIMENSIONS
    assert point.payload["family_id"] == family_id
    assert point.payload["summary"] == "This is a test summary."


@pytest.mark.asyncio
async def test_add_vectors_empty_data(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    await knowledge_qdrant_service.add_vectors([])
    mock_qdrant_client.upsert.assert_not_called()


@pytest.mark.asyncio
async def test_update_vectors_success(knowledge_qdrant_service, mock_embedding_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    entity_id = str(uuid.uuid4())
    point_id = f"{family_id}-{entity_id}"

    # Mock retrieve to return an existing point
    mock_qdrant_client.retrieve.return_value = [
        MagicMock(
            payload={"family_id": family_id, "entity_id": entity_id, "summary": "old summary", "age": 25},
            vector=[0.2] * TEXT_EMBEDDING_DIMENSIONS
        )
    ]
    
    update_request = UpdateVectorRequest(
        family_id=family_id,
        entity_id=entity_id,
        summary="new summary",
        metadata={"city": "New York"}
    )

    await knowledge_qdrant_service.update_vectors(update_request)

    mock_embedding_service.embed_query.assert_called_once_with("new summary")
    mock_qdrant_client.upsert.assert_called_once()
    args, kwargs = mock_qdrant_client.upsert.call_args
    points = args[0]['points'] if len(args) > 0 and 'points' in args[0] else kwargs['points']
    assert len(points) == 1
    point = points[0]
    assert point.id == point_id
    assert point.vector == [0.1] * TEXT_EMBEDDING_DIMENSIONS # New embedding
    assert point.payload["summary"] == "new summary"
    assert point.payload["age"] == 25 # Old metadata preserved
    assert point.payload["city"] == "New York" # New metadata added


@pytest.mark.asyncio
async def test_update_vectors_point_not_found(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    mock_qdrant_client.retrieve.return_value = [] # Point not found
    update_request = UpdateVectorRequest(
        family_id=str(uuid.uuid4()),
        entity_id=str(uuid.uuid4()),
        summary="new summary"
    )

    mock_qdrant_client.upsert.assert_not_called()


@pytest.mark.asyncio
async def test_delete_vectors_by_entity_id_success(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    entity_id = str(uuid.uuid4())
    delete_request = DeleteVectorRequest(family_id=family_id, entity_id=entity_id)

    result = await knowledge_qdrant_service.delete_vectors(delete_request)
    mock_qdrant_client.delete.assert_called_once()
    assert result == 1


@pytest.mark.asyncio
async def test_delete_vectors_by_type_success(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    delete_request = DeleteVectorRequest(family_id=family_id, type="member")

    result = await knowledge_qdrant_service.delete_vectors(delete_request)
    mock_qdrant_client.delete.assert_called_once()
    assert result == 1


@pytest.mark.asyncio
async def test_delete_knowledge_by_family_id_success(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    await knowledge_qdrant_service.delete_knowledge_by_family_id(family_id)
    mock_qdrant_client.delete.assert_called_once()


@pytest.mark.asyncio
async def test_rebuild_vectors_success(knowledge_qdrant_service, mock_embedding_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    entity_id = str(uuid.uuid4())
    rebuild_request = RebuildVectorRequest(family_id=family_id, entity_ids=[entity_id])

    # Mock scroll to return points to rebuild
    mock_qdrant_client.scroll.return_value = ([
        MagicMock(
            id=f"{family_id}-{entity_id}",
            payload={"family_id": family_id, "entity_id": entity_id, "summary": "old summary"},
            vector=[0.5] * TEXT_EMBEDDING_DIMENSIONS
        )
    ], None)

    await knowledge_qdrant_service.rebuild_vectors(rebuild_request)

    mock_embedding_service.embed_documents.assert_called_once_with(["old summary"])
    mock_qdrant_client.upsert.assert_called_once()
    args, kwargs = mock_qdrant_client.upsert.call_args
    points = args[0]['points'] if len(args) > 0 and 'points' in args[0] else kwargs['points']
    assert len(points) == 1
    point = points[0]
    assert point.id == f"{family_id}-{entity_id}"
    assert point.vector == [0.1] * TEXT_EMBEDDING_DIMENSIONS # New embedding
    assert point.payload["summary"] == "old summary" # Payload should remain same


@pytest.mark.asyncio
async def test_rebuild_vectors_no_entries(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    mock_qdrant_client.scroll.return_value = ([], None) # No points to rebuild
    rebuild_request = RebuildVectorRequest(family_id=str(uuid.uuid4()))

    await knowledge_qdrant_service.rebuild_vectors(rebuild_request)
    mock_qdrant_client.upsert.assert_not_called()


@pytest.mark.asyncio
async def test_search_knowledge_table_success(knowledge_qdrant_service, mock_qdrant_client):
    await knowledge_qdrant_service.async_init() # Initialize the service
    family_id = str(uuid.uuid4())
    query_vector = [0.1] * TEXT_EMBEDDING_DIMENSIONS
    allowed_visibility = ["public"]
    top_k = 1

    # Mock query_points to return results
    mock_qdrant_client.query_points.return_value = [
        MagicMock(
            id=f"{family_id}-entity1",
            score=0.9,
            payload={"family_id": family_id, "entity_id": "entity1", "summary": "found summary", "visibility": "public"}
        )
    ]

    results = await knowledge_qdrant_service.search_knowledge_table(
        family_id, query_vector, allowed_visibility, top_k
    )

    mock_qdrant_client.query_points.assert_called_once()
    assert len(results) == 1
    assert results[0]["score"] == 0.9
    assert results[0]["summary"] == "found summary"
    assert results[0]["metadata"]["entity_id"] == "entity1"
