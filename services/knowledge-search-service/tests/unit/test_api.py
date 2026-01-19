import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock
import numpy as np

# Import the main FastAPI app
from app.main import app
from app.config import TEXT_EMBEDDING_DIMENSIONS

# Create a test client for the FastAPI app
# client = TestClient(app) # Moved inside fixture for proper mocking

# Fixture to provide a TestClient
@pytest.fixture() # Removed autouse=True
def test_client():
    with TestClient(app) as client:
        yield client


def test_health_check(test_client):
    """
    Test the /health endpoint.
    """
    response = test_client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}

def test_search_endpoint_success(test_client, mocker):
    """
    Test the /api/v1/search endpoint with a successful response.
    Updated to match new SearchResultItem schema (with metadata).
    """
    mock_lancedb_service = mocker.patch('app.api.search.knowledge_lancedb_service', autospec=True)
    mock_embedding_service = mocker.patch('app.api.search.embedding_service', autospec=True)

    # Mock the search_knowledge_table method to return sample results in the new format
    mock_lancedb_service.search_knowledge_table.return_value = [
        {"metadata": {"original_id": "M001", "name": "Nguyễn Văn A", "content_type": "member"}, "summary": "Ông tổ đời thứ 3", "score": 0.85},
        {"metadata": {"original_id": "M002", "name": "Trần Thị B", "content_type": "member"}, "summary": "Vợ ông A", "score": 0.75},
    ]
    # Ensure embedding_service.embed_query returns a predictable vector
    mock_embedding_service.embed_query.return_value = np.random.rand(TEXT_EMBEDDING_DIMENSIONS).tolist()

    request_payload = {
        "family_id": "F123",
        "query": "ai là người lập nên dòng họ",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = test_client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 200
    data = response.json()
    assert "results" in data
    assert len(data["results"]) == 2
    assert data["results"][0]["metadata"]["original_id"] == "M001"
    assert data["results"][0]["metadata"]["name"] == "Nguyễn Văn A"
    assert data["results"][0]["summary"] == "Ông tổ đời thứ 3"
    assert data["results"][0]["score"] == 0.85

    mock_embedding_service.embed_query.assert_called_once_with(request_payload["query"])
    mock_lancedb_service.search_knowledge_table.assert_called_once()

def test_search_endpoint_no_results(test_client, mocker):
    """
    Test the /api/v1/search endpoint when no results are found.
    """
    mock_lancedb_service = mocker.patch('app.api.search.knowledge_lancedb_service', autospec=True)
    mock_lancedb_service.search_knowledge_table.return_value = []

    request_payload = {
        "family_id": "F123",
        "query": "query without results",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = test_client.post("/api/v1/search", json=request_payload)
    data = response.json()
    assert "results" in data
    assert len(data["results"]) == 0
    mock_lancedb_service.search_knowledge_table.assert_called_once()

def test_search_endpoint_invalid_payload(test_client):
    """
    Test the /api/v1/search endpoint with an invalid request payload.
    """
    invalid_payload = {
        "family_id": "F123",
        "query": "some query",
        # "top_k" is missing, which has a default, but let's test a more fundamental error
        "allowed_visibility": ["invalid_visibility_type"] # Invalid literal for allowed_visibility
    }

    response = test_client.post("/api/v1/search", json=invalid_payload)
    assert response.status_code == 422 # Unprocessable Entity for Pydantic validation errors

def test_search_endpoint_internal_server_error(test_client, mocker):
    """
    Test the /api/v1/search endpoint when an internal server error occurs in LanceDBService.
    """
    mock_lancedb_service = mocker.patch('app.api.search.knowledge_lancedb_service', autospec=True)
    mock_lancedb_service.search_knowledge_table.side_effect = Exception("LanceDB internal error")

    request_payload = {
        "family_id": "F123",
        "query": "query that causes error",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = test_client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 500
    assert response.json() == {"detail": "An internal server error occurred during search."}
    mock_lancedb_service.search_knowledge_table.assert_called_once()

# --- New tests for /api/v1/knowledge/index endpoint ---
from app.schemas.knowledge_dtos import GenericKnowledgeDto, KnowledgeAddRequest
from app.schemas.vectors import VectorData, DeleteVectorRequest

def test_add_knowledge_data_success(test_client, mocker):
    """
    Test the /api/v1/knowledge endpoint for a successful addition of data.
    """
    mock_lancedb_service = mocker.patch('app.api.knowledge.knowledge_lancedb_service', autospec=True)
    mock_embedding_service = mocker.patch('app.api.knowledge.embedding_service', autospec=True)
    
    sample_metadata = {
        "family_id": "F123",
        "original_id": "E456",
        "content_type": "event",
        "name": "Lễ giỗ tổ",
        "visibility": "public"
    }
    mock_lancedb_service._get_knowledge_table_name.return_value = f"family_knowledge_{sample_metadata['family_id']}"
    mock_embedding_service.embed_query.return_value = np.random.rand(TEXT_EMBEDDING_DIMENSIONS).tolist()
    request_payload = KnowledgeAddRequest(
        data=GenericKnowledgeDto(metadata=sample_metadata, summary="Đây là tóm tắt lễ giỗ tổ"),
    ).model_dump(mode='json')

    response = test_client.post("/api/v1/knowledge", json=request_payload)

    assert response.status_code == 201
    assert response.json() == {"message": "event data with original_id E456 added successfully for family F123."}
    
    # Assert that add_vectors was called with the correct VectorData
    mock_lancedb_service.add_vectors.assert_called_once()
    called_args, called_kwargs = mock_lancedb_service.add_vectors.call_args
    assert called_args[0] == f"family_knowledge_{sample_metadata['family_id']}" # table_name
    
    # Check the VectorData object passed
    vector_data_list = called_args[1]
    assert len(vector_data_list) == 1
    v_data = vector_data_list[0]
    assert isinstance(v_data, VectorData)
    assert v_data.family_id == "F123"
    assert v_data.entity_id == "E456"
    assert v_data.type == "event"
    assert v_data.visibility == "public"
    assert v_data.name == "Lễ giỗ tổ"
    assert v_data.summary == "Đây là tóm tắt lễ giỗ tổ"
    assert v_data.metadata == sample_metadata # Ensure full metadata is passed

def test_delete_knowledge_data_success(test_client, mocker):
    """
    Test the /api/v1/knowledge/{family_id}/{original_id} endpoint for a successful deletion.
    """
    mock_lancedb_service = mocker.patch('app.api.knowledge.knowledge_lancedb_service', autospec=True)
    
    # Mock delete_vectors to indicate a successful deletion
    family_id = "F123"
    original_id = "M789"

    mock_lancedb_service._get_knowledge_table_name.return_value = f"family_knowledge_{family_id}"
    mock_lancedb_service.delete_vectors.return_value = 1  # 1 item deleted

    response = test_client.delete(f"/api/v1/knowledge/{family_id}/{original_id}")

    assert response.status_code == 200
    assert response.json() == {"message": f"Knowledge data with original_id '{original_id}' deleted successfully for family '{family_id}'."}
    
    mock_lancedb_service.delete_vectors.assert_called_once()
    called_args, called_kwargs = mock_lancedb_service.delete_vectors.call_args
    assert called_args[0] == f"family_knowledge_{family_id}" # table_name
    
    delete_request_obj = called_args[1]
    assert isinstance(delete_request_obj, DeleteVectorRequest)
    assert delete_request_obj.family_id == family_id
    assert delete_request_obj.entity_id == original_id
    assert delete_request_obj.where_clause is None


def test_delete_knowledge_data_not_found(test_client, mocker):
    """
    Test the /api/v1/knowledge/{family_id}/{original_id} endpoint when no data is found for deletion.
    """
    mock_lancedb_service = mocker.patch('app.api.knowledge.knowledge_lancedb_service', autospec=True)
    mock_lancedb_service.delete_vectors.return_value = 0  # 0 items deleted

    family_id = "F456"
    original_id = "X007"

    response = test_client.delete(f"/api/v1/knowledge/{family_id}/{original_id}")

    assert response.status_code == 404
    assert response.json() == {"detail": f"No knowledge found with original_id '{original_id}' for family '{family_id}' to delete."}
    mock_lancedb_service.delete_vectors.assert_called_once()


