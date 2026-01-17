import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock
import numpy as np

# Import the main FastAPI app
from services.knowledge_search_service.app.main import app
from services.knowledge_search_service.app.config import EMBEDDING_DIMENSIONS

# Create a test client for the FastAPI app
client = TestClient(app)

# Fixture to ensure LanceDBService and EmbeddingService are clean for each test
@pytest.fixture(autouse=True)
def mock_services_for_api_tests():
    with patch('services.knowledge_search_service.app.main.lancedb_service') as mock_lancedb_service, \
         patch('services.knowledge_search_service.app.main.embedding_service') as mock_embedding_service:
        
        # Mock LanceDBService's create_dummy_table to prevent actual file system interaction
        mock_lancedb_service.create_dummy_table.return_value = None
        
        # Ensure embedding_service.embed_query returns a predictable vector
        mock_embedding_service.embed_query.return_value = np.random.rand(EMBEDDING_DIMENSIONS).tolist()
        
        yield mock_lancedb_service, mock_embedding_service

def test_health_check():
    """
    Test the /health endpoint.
    """
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}

def test_search_endpoint_success(mock_services_for_api_tests):
    """
    Test the /api/v1/search endpoint with a successful response.
    """
    mock_lancedb_service, mock_embedding_service = mock_services_for_api_tests

    # Mock the search_family_table method to return sample results
    mock_lancedb_service.search_family_table.return_value = [
        {"member_id": "M001", "name": "Nguyễn Văn A", "summary": "Ông tổ đời thứ 3", "score": 0.85},
        {"member_id": "M002", "name": "Trần Thị B", "summary": "Vợ ông A", "score": 0.75},
    ]

    request_payload = {
        "family_id": "F123",
        "query": "ai là người lập nên dòng họ",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 200
    data = response.json()
    assert "results" in data
    assert len(data["results"]) == 2
    assert data["results"][0]["member_id"] == "M001"
    assert data["results"][0]["name"] == "Nguyễn Văn A"
    assert data["results"][0]["summary"] == "Ông tổ đời thứ 3"
    assert data["results"][0]["score"] == 0.85

    mock_embedding_service.embed_query.assert_called_once_with(request_payload["query"])
    mock_lancedb_service.search_family_table.assert_called_once()

def test_search_endpoint_no_results(mock_services_for_api_tests):
    """
    Test the /api/v1/search endpoint when no results are found.
    """
    mock_lancedb_service, _ = mock_services_for_api_tests
    mock_lancedb_service.search_family_table.return_value = []

    request_payload = {
        "family_id": "F123",
        "query": "query without results",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 200
    data = response.json()
    assert "results" in data
    assert len(data["results"]) == 0
    mock_lancedb_service.search_family_table.assert_called_once()

def test_search_endpoint_invalid_payload():
    """
    Test the /api/v1/search endpoint with an invalid request payload.
    """
    invalid_payload = {
        "family_id": "F123",
        "query": "some query",
        # "top_k" is missing, which has a default, but let's test a more fundamental error
        "allowed_visibility": ["invalid_visibility_type"] # Invalid literal for allowed_visibility
    }

    response = client.post("/api/v1/search", json=invalid_payload)
    assert response.status_code == 422 # Unprocessable Entity for Pydantic validation errors

def test_search_endpoint_internal_server_error(mock_services_for_api_tests):
    """
    Test the /api/v1/search endpoint when an internal server error occurs in LanceDBService.
    """
    mock_lancedb_service, _ = mock_services_for_api_tests
    mock_lancedb_service.search_family_table.side_effect = Exception("LanceDB internal error")

    request_payload = {
        "family_id": "F123",
        "query": "query that causes error",
        "top_k": 5,
        "allowed_visibility": ["public"]
    }

    response = client.post("/api/v1/search", json=request_payload)

    assert response.status_code == 500
    assert response.json() == {"detail": "An internal server error occurred during search."}
    mock_lancedb_service.search_family_table.assert_called_once()
