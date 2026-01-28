import pytest
from unittest.mock import Mock, patch
from app.services.face_service import FaceService
from PIL import Image
import io
import base64
from qdrant_client.http.models import UpdateStatus

@pytest.fixture
def mock_qdrant_service():
    """Fixture for a mocked QdrantService."""
    return Mock()

@pytest.fixture
def mock_face_embedding_service():
    """Fixture for a mocked FaceEmbeddingService."""
    mock_service = Mock()
    mock_service.get_embedding.return_value = [0.1] * 128  # Return a dummy embedding
    return mock_service

@pytest.fixture
def face_service_instance(mock_qdrant_service, mock_face_embedding_service):
    """Fixture for a FaceService instance with mocked dependencies."""
    return FaceService(
        qdrant_service=mock_qdrant_service,
        face_embedding_service=mock_face_embedding_service
    )

@pytest.fixture
def dummy_image():
    """Returns a dummy PIL Image."""
    img = Image.new('RGB', (100, 100), color = 'red')
    return img

def test_add_face_success(face_service_instance, mock_qdrant_service, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng add_face thành công.
    """
    metadata = {
        "memberId": "member123",
        "familyId": "familyabc",
        "localDbId": "local123",
        "thumbnailUrl": "http://example.com/thumb.png",
        "originalImageUrl": "http://example.com/orig.jpg",
        "emotion": "happy",
        "emotionConfidence": 0.9
    }
    
    result = face_service_instance.add_face(dummy_image, metadata)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_service.upsert_face_embedding.assert_called_once()
    
    # Check upsert_face_embedding arguments
    args, kwargs = mock_qdrant_service.upsert_face_embedding.call_args
    assert args[0] == [0.1] * 128  # The dummy embedding
    assert args[1]["memberId"] == metadata["memberId"]
    assert args[1]["familyId"] == metadata["familyId"]
    assert "faceId" in args[1] # faceId should be added to metadata
    assert args[2] == args[1]["faceId"] # point_id should be the faceId
    
    assert "faceId" in result
    assert result["embedding"] == [0.1] * 128
    assert result["metadata"]["memberId"] == metadata["memberId"]

def test_add_face_missing_metadata(face_service_instance, dummy_image):
    """
    Kiểm tra add_face khi thiếu memberId hoặc familyId trong metadata.
    """
    with pytest.raises(ValueError, match="Metadata phải chứa 'memberId' và 'familyId'."):
        face_service_instance.add_face(dummy_image, {"localDbId": "local123"})

def test_get_faces_by_family_id(face_service_instance, mock_qdrant_service):
    """
    Kiểm tra chức năng get_faces_by_family_id.
    """
    family_id = "familyabc"
    mock_qdrant_service.get_points_by_payload_filter.return_value = [
        {"id": "face1", "payload": {"familyId": family_id, "memberId": "mem1"}},
        {"id": "face2", "payload": {"familyId": family_id, "memberId": "mem2"}}
    ]
    
    faces = face_service_instance.get_faces_by_family_id(family_id)
    
    mock_qdrant_service.get_points_by_payload_filter.assert_called_once_with(
        payload_filter={"familyId": family_id}
    )
    assert len(faces) == 2
    assert faces[0]["id"] == "face1"
    assert faces[1]["payload"]["memberId"] == "mem2"

def test_delete_face_success(face_service_instance, mock_qdrant_service):
    """
    Kiểm tra chức năng delete_face thành công.
    """
    face_id = "face_to_delete"
    mock_qdrant_service.delete_point_by_id.return_value = True
    
    success = face_service_instance.delete_face(face_id)
    
    mock_qdrant_service.delete_point_by_id.assert_called_once_with(face_id)
    assert success is True

def test_delete_face_failure(face_service_instance, mock_qdrant_service):
    """
    Kiểm tra chức năng delete_face thất bại.
    """
    face_id = "face_to_delete_fail"
    mock_qdrant_service.delete_point_by_id.return_value = False
    
    success = face_service_instance.delete_face(face_id)
    
    mock_qdrant_service.delete_point_by_id.assert_called_once_with(face_id)
    assert success is False

def test_search_similar_faces(face_service_instance, mock_qdrant_service, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng search_similar_faces.
    """
    family_id = "family_search"
    limit = 3
    query_embedding = [0.1] * 128
    mock_face_embedding_service.get_embedding.return_value = query_embedding
    
    mock_qdrant_service.search_face_embeddings.return_value = [
        {"id": "res1", "score": 0.99, "payload": {"familyId": family_id}},
        {"id": "res2", "score": 0.98, "payload": {"familyId": family_id}},
    ]
    
    results = face_service_instance.search_similar_faces(dummy_image, family_id, limit)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_service.search_face_embeddings.assert_called_once_with(
        query_vector=query_embedding,
        limit=limit,
        query_filter={"familyId": family_id}
    )
    assert len(results) == 2
    assert results[0]["id"] == "res1"
    assert results[1]["score"] == 0.98

def test_search_similar_faces_no_family_id(face_service_instance, mock_qdrant_service, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng search_similar_faces khi không có family_id.
    """
    limit = 5
    query_embedding = [0.1] * 128
    mock_face_embedding_service.get_embedding.return_value = query_embedding
    
    mock_qdrant_service.search_face_embeddings.return_value = [
        {"id": "res1", "score": 0.99, "payload": {}},
    ]
    
    results = face_service_instance.search_similar_faces(dummy_image, None, limit)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_service.search_face_embeddings.assert_called_once_with(
        query_vector=query_embedding,
        limit=limit,
        query_filter=None
    )
    assert len(results) == 1
    assert results[0]["id"] == "res1"

def test_add_face_by_vector_success(face_service_instance, mock_qdrant_service):
    """
    Kiểm tra chức năng add_face_by_vector thành công.
    """
    vector = [0.2] * 128
    metadata = {
        "memberId": "member456",
        "familyId": "familyxyz",
        "localDbId": "local456",
        "thumbnailUrl": "http://example.com/thumb2.png",
        "originalImageUrl": "http://example.com/orig2.jpg",
        "emotion": "sad",
        "emotionConfidence": 0.7
    }

    result = face_service_instance.add_face_by_vector(vector, metadata)

    mock_qdrant_service.upsert_face_embedding.assert_called_once()
    
    args, kwargs = mock_qdrant_service.upsert_face_embedding.call_args
    assert args[0] == vector
    assert args[1]["memberId"] == metadata["memberId"]
    assert args[1]["familyId"] == metadata["familyId"]
    assert "faceId" in args[1]
    assert args[2] == args[1]["faceId"]

    assert "faceId" in result
    assert result["embedding"] == vector
    assert result["metadata"]["memberId"] == metadata["memberId"]

def test_add_face_by_vector_missing_metadata(face_service_instance):
    """
    Kiểm tra add_face_by_vector khi thiếu memberId hoặc familyId trong metadata.
    """
    vector = [0.2] * 128
    with pytest.raises(ValueError, match="Metadata phải chứa 'memberId' và 'familyId'."):
        face_service_instance.add_face_by_vector(vector, {"localDbId": "local456"})