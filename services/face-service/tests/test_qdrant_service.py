import os
import pytest
from unittest.mock import Mock, patch
from qdrant_client import models
from qdrant_client.http.models import UpdateStatus # Import UpdateStatus for testing
from src.infrastructure.persistence.qdrant_client import QdrantFaceRepository
from src.domain.interfaces.face_repository import IFaceRepository

@pytest.fixture
def mock_qdrant_client():
    """Fixture to provide a mocked QdrantClient instance."""
    with patch('src.infrastructure.persistence.qdrant_client.QdrantClient') as MockClient:
        mock_instance = MockClient.return_value
        yield mock_instance

@pytest.fixture
def qdrant_repository_instance(mock_qdrant_client, monkeypatch):
    """Fixture to provide a QdrantFaceRepository instance with a mocked client."""
    monkeypatch.setenv("QDRANT_HOST", "http://localhost:6333")
    monkeypatch.setenv("QDRANT_API_KEY", "test_api_key")
    monkeypatch.setenv("QDRANT_COLLECTION_NAME", "env_test_collection")
    monkeypatch.setenv("QDRANT_VECTOR_SIZE", "128") # Ensure vector size is set

    mock_qdrant_client.collection_exists.return_value = False # Force collection creation logic
    service = QdrantFaceRepository(collection_name=None)
    mock_qdrant_client.collection_exists.return_value = True # For subsequent checks, assume it exists
    yield service


def test_qdrant_repository_init_creates_collection(mock_qdrant_client, monkeypatch):
    """
    Kiểm tra rằng collection được tạo nếu nó chưa tồn tại khi collection_name không được cung cấp.
    """
    monkeypatch.setenv("QDRANT_HOST", "http://localhost:6333")
    monkeypatch.setenv("QDRANT_API_KEY", "test_api_key")
    monkeypatch.setenv("QDRANT_COLLECTION_NAME", "new_env_collection")
    monkeypatch.setenv("QDRANT_VECTOR_SIZE", "128")

    mock_qdrant_client.collection_exists.return_value = False
    
    repository = QdrantFaceRepository(collection_name=None)
    
    mock_qdrant_client.collection_exists.assert_called_once_with(collection_name="new_env_collection")
    mock_qdrant_client.create_collection.assert_called_once_with(
        collection_name="new_env_collection",
        vectors_config=models.VectorParams(size=128, distance=models.Distance.COSINE),
    )
    mock_qdrant_client.create_payload_index.assert_called_once_with(
        collection_name="new_env_collection",
        field_name="family_id",
        field_schema=models.PayloadSchemaType.KEYWORD,
    )
    assert repository.collection_name == "new_env_collection"


def test_qdrant_repository_init_does_not_recreate_existing_collection(mock_qdrant_client, monkeypatch):
    """
    Kiểm tra rằng collection không được tạo lại nếu nó đã tồn tại khi collection_name không được cung cấp.
    """
    monkeypatch.setenv("QDRANT_HOST", "http://localhost:6333")
    monkeypatch.setenv("QDRANT_API_KEY", "test_api_key")
    monkeypatch.setenv("QDRANT_COLLECTION_NAME", "existing_env_collection")
    monkeypatch.setenv("QDRANT_VECTOR_SIZE", "128")

    mock_qdrant_client.collection_exists.return_value = True
    
    repository = QdrantFaceRepository(collection_name=None)
    
    mock_qdrant_client.collection_exists.assert_called_once_with(collection_name="existing_env_collection")
    mock_qdrant_client.create_collection.assert_not_called()
    mock_qdrant_client.recreate_collection.assert_not_called()
    mock_qdrant_client.delete_collection.assert_not_called()
    mock_qdrant_client.create_payload_index.assert_not_called()
    assert repository.collection_name == "existing_env_collection"

@pytest.mark.asyncio
async def test_upsert_face_vector(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức upsert_face_vector.
    """
    vector = [0.1] * 128
    metadata = {"memberId": "123", "familyId": "abc"}
    face_id = "test_face_id"
    
    await qdrant_repository_instance.upsert_face_vector(face_id, vector, metadata)
    
    mock_qdrant_client.upsert.assert_called_once()
    args, kwargs = mock_qdrant_client.upsert.call_args
    assert kwargs['collection_name'] == qdrant_repository_instance.collection_name
    assert len(kwargs['points']) == 1
    assert kwargs['points'][0].id == face_id
    assert kwargs['points'][0].vector == vector
    assert kwargs['points'][0].payload == metadata

@pytest.mark.asyncio
async def test_search_similar_faces(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức search_similar_faces.
    """
    query_vector = [0.2] * 128
    
    mock_hit = Mock()
    mock_hit.id = "found_id"
    mock_hit.score = 0.95
    mock_hit.payload = {"memberId": "456", "familyId": "def"}
    
    mock_query_result = Mock()
    mock_query_result.points = [mock_hit]
    mock_qdrant_client.query_points.return_value = mock_query_result
    
    results = await qdrant_repository_instance.search_similar_faces(query_vector, threshold=0.0)
    
    mock_qdrant_client.query_points.assert_called_once_with(
        collection_name=qdrant_repository_instance.collection_name,
        query=query_vector,
        limit=5,
        query_filter=None,
        score_threshold=0.0
    )
    assert len(results) == 1
    assert results[0]["id"] == "found_id"
    assert results[0]["score"] == 0.95
    assert results[0]["payload"] == {"memberId": "456", "familyId": "def"}

@pytest.mark.asyncio
async def test_search_similar_faces_with_filter(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức search_similar_faces với bộ lọc.
    """
    query_vector = [0.2] * 128
    family_id = "family_xyz"
    
    mock_hit = Mock()
    mock_hit.id = "filtered_id"
    mock_hit.score = 0.98
    mock_hit.payload = {"member_id": "789", "family_id": family_id}
    
    mock_query_result = Mock()
    mock_query_result.points = [mock_hit]
    mock_qdrant_client.query_points.return_value = mock_query_result
    
    results = await qdrant_repository_instance.search_similar_faces(query_vector, family_id=family_id, threshold=0.0)
    
    mock_qdrant_client.query_points.assert_called_once()
    args, kwargs = mock_qdrant_client.query_points.call_args
    assert kwargs['collection_name'] == qdrant_repository_instance.collection_name
    assert kwargs['query'] == query_vector
    assert kwargs['limit'] == 5
    assert kwargs['score_threshold'] == 0.0
    assert isinstance(kwargs['query_filter'], models.Filter)
    assert len(kwargs['query_filter'].must) == 1
    assert kwargs['query_filter'].must[0].key == "family_id"
    assert kwargs['query_filter'].must[0].match.value == family_id
    assert len(results) == 1
    assert results[0]["id"] == "filtered_id"


@pytest.mark.asyncio
async def test_get_faces_by_family_id(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức get_faces_by_family_id.
    """
    family_id = "test_family"
    
    mock_hit = Mock()
    mock_hit.id = "scroll_id"
    mock_hit.payload = {"familyId": family_id, "memberId": "member_xyz"}
    mock_qdrant_client.scroll.return_value = ([mock_hit], None) # (hits, next_page_offset)
    
    results = await qdrant_repository_instance.get_faces_by_family_id(family_id)
    
    mock_qdrant_client.scroll.assert_called_once()
    args, kwargs = mock_qdrant_client.scroll.call_args
    assert isinstance(kwargs['scroll_filter'], models.Filter)
    assert len(kwargs['scroll_filter'].must) == 1
    assert kwargs['scroll_filter'].must[0].key == "family_id" # Corrected key
    assert kwargs['scroll_filter'].must[0].match.value == family_id
    assert len(results) == 1
    assert results[0]["id"] == "scroll_id"
    assert results[0]["payload"] == {"familyId": family_id, "memberId": "member_xyz"}

@pytest.mark.asyncio
async def test_delete_face_success(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức delete_face khi xóa thành công.
    """
    face_id = "point_to_delete"
    mock_response = Mock()
    mock_response.status = UpdateStatus.COMPLETED
    mock_qdrant_client.delete.return_value = mock_response
    
    success = await qdrant_repository_instance.delete_face(face_id)
    
    mock_qdrant_client.delete.assert_called_once_with(
        collection_name=qdrant_repository_instance.collection_name,
        points_selector=models.PointIdsList(points=[face_id]), # Correct points argument
        wait=True
    )
    assert success is True

@pytest.mark.asyncio
async def test_delete_face_failure(qdrant_repository_instance, mock_qdrant_client):
    """
    Kiểm tra phương thức delete_face khi xóa thất bại.
    """
    face_id = "point_to_fail_delete"
    mock_response = Mock()
    mock_response.status = UpdateStatus.ACKNOWLEDGED
    mock_qdrant_client.delete.return_value = mock_response
    
    success = await qdrant_repository_instance.delete_face(face_id)
    
    mock_qdrant_client.delete.assert_called_once_with(
        collection_name=qdrant_repository_instance.collection_name,
        points_selector=models.PointIdsList(points=[face_id]), # Correct points argument
        wait=True
    )
    assert success is False


