from fastapi.testclient import TestClient
from unittest.mock import MagicMock, patch
import pytest
from app.main import app
from app.api.knowledge import get_knowledge_qdrant_service, get_embedding_service
from app.core.qdrant import KnowledgeQdrantService
from app.core.embeddings import EmbeddingService
from app.schemas.vectors import DeleteVectorRequest, VectorData
from app.schemas.knowledge_dtos import KnowledgeAddRequest, GenericKnowledgeDto # Corrected import
from uuid import UUID

# Mock dependencies
@pytest.fixture
def mock_knowledge_qdrant_service():
    service = MagicMock(spec=KnowledgeQdrantService)
    service.delete_vectors.return_value = 1 # Assume 1 item deleted for upsert test
    service.add_vectors.return_value = None # No return value for add_vectors
    # Mock the new method
    service.delete_knowledge_by_family_id.return_value = None
    return service

@pytest.fixture
def mock_embedding_service():
    service = MagicMock(spec=EmbeddingService)
    # Mock the embed_query method to return a dummy embedding
    service.embed_query.return_value = [0.1] * 1536 # Example dummy embedding
    return service

@pytest.fixture
def client(mock_knowledge_qdrant_service, mock_embedding_service):
    app.dependency_overrides[get_knowledge_qdrant_service] = lambda: mock_knowledge_qdrant_service
    app.dependency_overrides[get_embedding_service] = lambda: mock_embedding_service
    with patch('app.main.KnowledgeQdrantService', return_value=mock_knowledge_qdrant_service):
        with patch('app.main.global_embedding_service', new=mock_embedding_service):
            with TestClient(app) as c:
                yield c
    app.dependency_overrides.clear() # Clear overrides after test

def test_upsert_knowledge_data_success(client, mock_knowledge_qdrant_service):
    family_id = str(UUID("12345678-1234-5678-1234-567812345678"))
    original_id = str(UUID("87654321-4321-8765-4321-876543218765"))
    content_type = "document"
    summary = "This is a test summary."

    request_data = KnowledgeAddRequest(
        data=GenericKnowledgeDto( # Use GenericKnowledgeDto
            summary=summary,
            metadata={ # Pass metadata as a dictionary
                "family_id": family_id,
                "content_type": content_type,
                "original_id": original_id,
                "name": "Test Document",
                "visibility": "private",
            }
        )
    )

    response = client.post("/api/v1/knowledge/upsert", json=request_data.model_dump())

    assert response.status_code == 200
    assert response.json()["message"] == f"{content_type} data with original_id {original_id} upserted successfully for family {family_id}."

    # Assert delete_vectors was called with the correct filter
    mock_knowledge_qdrant_service.delete_vectors.assert_called_once()
    args, kwargs = mock_knowledge_qdrant_service.delete_vectors.call_args
    assert isinstance(args[0], DeleteVectorRequest)
    assert args[0].where_clause == (
        f"family_id = '{family_id}' AND "
        f"entity_id = '{original_id}' AND "
        f"type = '{content_type}'"
    )

    # Assert add_vectors was called with the correct data
    mock_knowledge_qdrant_service.add_vectors.assert_called_once()
    args, kwargs = mock_knowledge_qdrant_service.add_vectors.call_args
    assert len(args[0]) == 1 # List containing one VectorData object
    vector_data = args[0][0]
    assert isinstance(vector_data, VectorData)
    assert vector_data.family_id == family_id
    assert vector_data.entity_id == original_id
    assert vector_data.type == content_type
    assert vector_data.summary == summary
    assert vector_data.metadata["original_id"] == original_id

def test_upsert_knowledge_data_missing_metadata(client):
    request_data = KnowledgeAddRequest(
        data=GenericKnowledgeDto( # Use GenericKnowledgeDto
            summary="This is a test summary.",
            metadata={ # Pass metadata as a dictionary
                "family_id": str(UUID("12345678-1234-5678-1234-567812345678")),
                "content_type": "document",
                "name": "Test Document",
                "visibility": "private",
            }
        )
    )

    response = client.post("/api/v1/knowledge/upsert", json=request_data.model_dump())

    assert response.status_code == 400
    assert "Missing essential metadata" in response.json()["detail"]


def test_delete_knowledge_by_family_id_success(client, mock_knowledge_qdrant_service):
    family_id = str(UUID("12345678-1234-5678-1234-567812345678"))
    mock_knowledge_qdrant_service.delete_knowledge_by_family_id.return_value = None # It returns None for success

    response = client.delete(f"/api/v1/knowledge/family-data/{family_id}")

    assert response.status_code == 200
    assert response.json()["message"] == f"All knowledge data for family_id '{family_id}' deleted successfully."
    mock_knowledge_qdrant_service.delete_knowledge_by_family_id.assert_called_once_with(family_id)

def test_delete_knowledge_by_family_id_exception(client, mock_knowledge_qdrant_service):
    family_id = str(UUID("12345678-1234-5678-1234-567812345678"))
    mock_knowledge_qdrant_service.delete_knowledge_by_family_id.side_effect = Exception("Test exception")

    response = client.delete(f"/api/v1/knowledge/family-data/{family_id}")

    assert response.status_code == 500
    assert "Error deleting knowledge data" in response.json()["detail"]