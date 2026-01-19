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
    mock_lancedb_service = mocker.patch('app.api.search.lancedb_service', autospec=True)
    mock_embedding_service = mocker.patch('app.api.search.embedding_service', autospec=True)

    # Mock the search_family_table method to return sample results in the new format
    mock_lancedb_service.search_family_table.return_value = [
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
    mock_lancedb_service.search_family_table.assert_called_once()

def test_search_endpoint_no_results(test_client, mocker):
    """
    Test the /api/v1/search endpoint when no results are found.
    """
    mock_lancedb_service = mocker.patch('app.api.search.lancedb_service', autospec=True)
    mock_lancedb_service.search_family_table.return_value = []

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
    mock_lancedb_service.search_family_table.assert_called_once()

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
    mock_lancedb_service = mocker.patch('app.api.search.lancedb_service', autospec=True)
    mock_lancedb_service.search_family_table.side_effect = Exception("LanceDB internal error")

    request_payload = {
        "family_id": "F123",
        "query": "query that causes error",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = test_client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 500
    assert response.json() == {"detail": "An internal server error occurred during search."}
    mock_lancedb_service.search_family_table.assert_called_once()

# --- New tests for /api/v1/knowledge/index endpoint ---
from app.schemas.knowledge_dtos import GenericKnowledgeDto, KnowledgeIndexRequest
from app.schemas.vectors import VectorData, DeleteVectorRequest

def test_index_knowledge_data_success_index_action(test_client, mocker):
    """
    Test the /api/v1/knowledge/index endpoint for a successful 'index' action.
    """
    mock_lancedb_service = mocker.patch('app.api.knowledge.lancedb_service', autospec=True)
    mock_embedding_service = mocker.patch('app.api.knowledge.embedding_service', autospec=True)
    
    # Mock _get_table_name to return the expected table name
    mock_lancedb_service._get_table_name.return_value = "family_F123"

    mock_lancedb_service.add_vectors.return_value = None
    mock_embedding_service.embed_query.return_value = np.random.rand(TEXT_EMBEDDING_DIMENSIONS).tolist()

    sample_metadata = {
        "family_id": "F123",
        "original_id": "E456",
        "content_type": "event",
        "name": "Lễ giỗ tổ",
        "visibility": "public"
    }
    request_payload = KnowledgeIndexRequest(
        data=GenericKnowledgeDto(metadata=sample_metadata, summary="Đây là tóm tắt lễ giỗ tổ"),
        action="index"
    ).model_dump(mode='json') # Use mode='json' to handle UUIDs if any or complex types

    response = test_client.post("/api/v1/knowledge/index", json=request_payload)

    assert response.status_code == 200
    assert response.json() == {"message": "event data with original_id E456 indexed successfully for family F123."}
    
    # Assert that add_vectors was called with the correct VectorData
    mock_lancedb_service.add_vectors.assert_called_once()
    called_args, called_kwargs = mock_lancedb_service.add_vectors.call_args
    assert called_args[0] == "family_F123" # table_name
    
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

def test_index_knowledge_data_success_delete_action(test_client, mocker):
    """
    Test the /api/v1/knowledge/index endpoint for a successful 'delete' action.
    """
    mock_lancedb_service = mocker.patch('app.api.knowledge.lancedb_service', autospec=True)
    
    # Mock _get_table_name to return the expected table name
    mock_lancedb_service._get_table_name.return_value = "family_F123"
    mock_lancedb_service.delete_vectors.return_value = None

    sample_metadata = {
        "family_id": "F123",
        "original_id": "M789",
        "content_type": "member"
    }
    request_payload = KnowledgeIndexRequest(
        data=GenericKnowledgeDto(metadata=sample_metadata, summary=""), # Summary not critical for delete
        action="delete"
    ).model_dump(mode='json')

    response = test_client.post("/api/v1/knowledge/index", json=request_payload)

    assert response.status_code == 200
    assert response.json() == {"message": "member data with original_id M789 deleted successfully for family F123."}
    
    # Assert that delete_vectors was called with the correct DeleteVectorRequest
    mock_lancedb_service.delete_vectors.assert_called_once()
    called_args, called_kwargs = mock_lancedb_service.delete_vectors.call_args
    assert called_args[0] == "family_F123" # table_name
    
    delete_request_obj = called_args[1] # It's a DeleteVectorRequest object from knowledge.py
    assert isinstance(delete_request_obj, DeleteVectorRequest)
    assert delete_request_obj.family_id == "F123"
    assert delete_request_obj.where_clause == "original_id = 'M789'" # Verify where_clause based on original_id

def test_index_knowledge_data_missing_essential_metadata_fields(test_client):
    """
    Test the /api/v1/knowledge/index endpoint with missing essential metadata fields.
    """
    invalid_metadata = {
        "family_id": "F123",
        # "original_id" is missing
        "content_type": "event"
    }
    request_payload = KnowledgeIndexRequest(
        data=GenericKnowledgeDto(metadata=invalid_metadata, summary="Some summary"),
        action="index"
    ).model_dump(mode='json')

    response = test_client.post("/api/v1/knowledge/index", json=request_payload)
    assert response.status_code == 400
    assert response.json()["detail"] == "Missing essential metadata: family_id, content_type, or original_id."

def test_index_knowledge_data_invalid_action(test_client):
    """
    Test the /api/v1/knowledge/index endpoint with an invalid action.
    """
    sample_metadata = {
        "family_id": "F123",
        "original_id": "E456",
        "content_type": "event"
    }
    request_payload = KnowledgeIndexRequest(
        data=GenericKnowledgeDto(metadata=sample_metadata, summary="Some summary"),
        action="invalid_action"
    ).model_dump(mode='json')

    response = test_client.post("/api/v1/knowledge/index", json=request_payload)
    assert response.status_code == 400
    assert response.json()["detail"] == "Invalid action specified."
