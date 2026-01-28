import os
import pytest
from unittest.mock import Mock, patch
from qdrant_client import models
from qdrant_client.http.models import UpdateStatus # Import UpdateStatus for testing
from app.services.qdrant_service import QdrantService


@pytest.fixture
def mock_qdrant_client():
    """Fixture to provide a mocked QdrantClient instance."""
    with patch('app.services.qdrant_service.QdrantClient') as MockClient:
        mock_instance = MockClient.return_value
        # Mock get_collection to raise an exception by default (collection not found)
        mock_instance.get_collection.side_effect = Exception("Collection not found")
        yield mock_instance

@pytest.fixture
def qdrant_service_instance(mock_qdrant_client, monkeypatch):
    """Fixture to provide a QdrantService instance with a mocked client."""
    # Temporarily set environment variables for the test using monkeypatch
    monkeypatch.setenv("QDRANT_HOST", "http://localhost:6333")
    monkeypatch.setenv("QDRANT_API_KEY", "test_api_key")
    monkeypatch.setenv("QDRANT_COLLECTION_NAME", "env_test_collection") # Set the environment variable
    
    # Ensure get_collection does not raise an exception for the service init
    mock_qdrant_client.get_collection.side_effect = None 
    mock_qdrant_client.get_collection.return_value = Mock() # Return a dummy object for existing collection
    service = QdrantService(collection_name=None) # Pass None to use environment variable
    yield service


def test_qdrant_service_init_creates_collection(mock_qdrant_client):
    """
    Kiểm tra rằng collection được tạo nếu nó chưa tồn tại khi collection_name không được cung cấp.
    """
    # Configure mock to simulate collection not found
    mock_qdrant_client.get_collection.side_effect = Exception("Collection not found")
    
    with patch('os.getenv', return_value="new_env_collection"):
        service = QdrantService() # No collection_name provided
    
    mock_qdrant_client.delete_collection.assert_called_once_with(collection_name="new_env_collection")
    mock_qdrant_client.recreate_collection.assert_called_once_with(
        collection_name="new_env_collection",
        vectors_config=models.VectorParams(size=128, distance=models.Distance.COSINE),
    )
    assert service.collection_name == "new_env_collection"


def test_qdrant_service_init_does_not_recreate_existing_collection(mock_qdrant_client):
    """
    Kiểm tra rằng collection không được tạo lại nếu nó đã tồn tại khi collection_name không được cung cấp.
    """
    # Configure mock to simulate collection found
    mock_qdrant_client.get_collection.side_effect = None # Reset side effect
    mock_qdrant_client.get_collection.return_value = Mock() # Return a dummy object
    
    with patch('os.getenv', return_value="existing_env_collection"):
        service = QdrantService() # No collection_name provided
    
    mock_qdrant_client.delete_collection.assert_called_once_with(collection_name="existing_env_collection")
    mock_qdrant_client.recreate_collection.assert_called_once_with(
        collection_name="existing_env_collection",
        vectors_config=models.VectorParams(size=128, distance=models.Distance.COSINE),
    )
    assert service.collection_name == "existing_env_collection"

def test_upsert_face_embedding(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức upsert_face_embedding.
    """
    vector = [0.1] * 128
    metadata = {"memberId": "123", "familyId": "abc"}
    point_id = "test_point_id"
    
    qdrant_service_instance.upsert_face_embedding(vector, metadata, point_id)
    
    mock_qdrant_client.upsert.assert_called_once()
    args, kwargs = mock_qdrant_client.upsert.call_args
    assert kwargs['collection_name'] == qdrant_service_instance.collection_name
    assert len(kwargs['points']) == 1
    assert kwargs['points'][0].id == point_id
    assert kwargs['points'][0].vector == vector
    assert kwargs['points'][0].payload == metadata

def test_search_face_embeddings(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức search_face_embeddings.
    """
    query_vector = [0.2] * 128
    
    mock_hit = Mock()
    mock_hit.id = "found_id"
    mock_hit.score = 0.95
    mock_hit.payload = {"memberId": "456", "familyId": "def"}
    
    mock_query_result = Mock()
    mock_query_result.points = [mock_hit]
    mock_qdrant_client.query_points.return_value = mock_query_result
    
    results = qdrant_service_instance.search_face_embeddings(query_vector, score_threshold=0.0)
    
    mock_qdrant_client.query_points.assert_called_once_with(
        collection_name=qdrant_service_instance.collection_name,
        query=query_vector,
        limit=5,
        query_filter=None,
        score_threshold=0.0
    )
    assert len(results) == 1
    assert results[0]["id"] == "found_id"
    assert results[0]["score"] == 0.95
    assert results[0]["payload"] == {"memberId": "456", "familyId": "def"}

def test_search_face_embeddings_with_filter(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức search_face_embeddings với bộ lọc.
    """
    query_vector = [0.2] * 128
    query_filter = {"family_id": "family_xyz"}
    
    mock_hit = Mock()
    mock_hit.id = "filtered_id"
    mock_hit.score = 0.98
    mock_hit.payload = {"member_id": "789", "family_id": "family_xyz"}
    
    mock_query_result = Mock()
    mock_query_result.points = [mock_hit]
    mock_qdrant_client.query_points.return_value = mock_query_result
    
    results = qdrant_service_instance.search_face_embeddings(query_vector, query_filter=query_filter, score_threshold=0.0)
    
    mock_qdrant_client.query_points.assert_called_once()
    args, kwargs = mock_qdrant_client.query_points.call_args
    assert kwargs['collection_name'] == qdrant_service_instance.collection_name
    assert kwargs['query'] == query_vector
    assert kwargs['limit'] == 5
    assert kwargs['score_threshold'] == 0.0
    assert isinstance(kwargs['query_filter'], models.Filter)
    assert len(kwargs['query_filter'].must) == 1
    assert kwargs['query_filter'].must[0].key == "family_id"
    assert kwargs['query_filter'].must[0].match.value == "family_xyz"
    assert len(results) == 1
    assert results[0]["id"] == "filtered_id"


def test_get_points_by_payload_filter(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức get_points_by_payload_filter.
    """
    payload_filter = {"familyId": "test_family"}
    
    mock_hit = Mock()
    mock_hit.id = "scroll_id"
    mock_hit.payload = {"familyId": "test_family", "memberId": "member_xyz"}
    mock_qdrant_client.scroll.return_value = ([mock_hit], None) # (hits, next_page_offset)
    
    results = qdrant_service_instance.get_points_by_payload_filter(payload_filter)
    
    mock_qdrant_client.scroll.assert_called_once()
    args, kwargs = mock_qdrant_client.scroll.call_args
    assert isinstance(kwargs['scroll_filter'], models.Filter)
    assert len(kwargs['scroll_filter'].must) == 1
    assert kwargs['scroll_filter'].must[0].key == "familyId"
    assert kwargs['scroll_filter'].must[0].match.value == "test_family"
    assert len(results) == 1
    assert results[0]["id"] == "scroll_id"
    assert results[0]["payload"] == {"familyId": "test_family", "memberId": "member_xyz"}

def test_delete_point_by_id_success(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức delete_point_by_id khi xóa thành công.
    """
    point_id = "point_to_delete"
    mock_response = Mock()
    mock_response.status = UpdateStatus.COMPLETED # Use imported UpdateStatus
    mock_qdrant_client.delete.return_value = mock_response
    
    success = qdrant_service_instance.delete_point_by_id(point_id)
    
    mock_qdrant_client.delete.assert_called_once_with(
        collection_name=qdrant_service_instance.collection_name,
        points=[point_id], # Correct points argument
        wait=True
    )
    assert success is True

def test_delete_point_by_id_failure(qdrant_service_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức delete_point_by_id khi xóa thất bại.
    """
    point_id = "point_to_fail_delete"
    mock_response = Mock()
    mock_response.status = UpdateStatus.ACKNOWLEDGED # Use imported UpdateStatus
    mock_qdrant_client.delete.return_value = mock_response
    
    success = qdrant_service_instance.delete_point_by_id(point_id)
    
    mock_qdrant_client.delete.assert_called_once_with(
        collection_name=qdrant_service_instance.collection_name,
        points=[point_id], # Correct points argument
        wait=True
    )
    assert success is False

