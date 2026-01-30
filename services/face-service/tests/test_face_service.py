import pytest
from unittest.mock import Mock, patch, AsyncMock
from src.application.services.face_manager import FaceManager
from src.infrastructure.persistence.qdrant_client import QdrantFaceRepository
from src.infrastructure.embeddings.facenet_embedding import FaceNetEmbeddingService
from PIL import Image
import io
import base64
from qdrant_client.http.models import UpdateStatus

@pytest.fixture
def mock_qdrant_repository():
    """Fixture for a mocked QdrantFaceRepository."""
    mock = AsyncMock(spec=QdrantFaceRepository)
    mock.upsert_face_vector = AsyncMock(return_value=None)
    mock.get_faces_by_family_id = AsyncMock(return_value=[])
    mock.delete_face = AsyncMock(return_value=True)
    mock.delete_faces_by_family_id = AsyncMock(return_value=True)
    mock.search_similar_faces = AsyncMock(return_value=[])
    return mock

@pytest.fixture
def mock_face_embedding_service():
    """Fixture for a mocked FaceNetEmbeddingService."""
    mock_service = Mock(spec=FaceNetEmbeddingService)
    mock_service.get_embedding.return_value = [0.1] * 128  # Return a dummy embedding
    return mock_service

@pytest.fixture
def face_manager_instance(mock_qdrant_repository, mock_face_embedding_service):
    """Fixture for a FaceManager instance with mocked dependencies."""
    return FaceManager(
        face_repository=mock_qdrant_repository,
        face_embedding_service=mock_face_embedding_service
    )

@pytest.fixture
def dummy_image():
    """Returns a dummy PIL Image."""
    img = Image.new('RGB', (100, 100), color = 'red')
    return img

@pytest.mark.asyncio
async def test_add_face_success(face_manager_instance, mock_qdrant_repository, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng add_face thành công.
    """
    metadata = {
        "member_id": "member123",
        "family_id": "familyabc",
        "localDbId": "local123",
        "thumbnailUrl": "http://example.com/thumb.png",
        "originalImageUrl": "http://example.com/orig.jpg",
        "emotion": "happy",
        "emotionConfidence": 0.9,
        "face_id": "face123"
    }    
    result = await face_manager_instance.add_face(dummy_image, metadata)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_repository.upsert_face_vector.assert_called_once()
    
    # Check upsert_face_embedding arguments
    args, kwargs = mock_qdrant_repository.upsert_face_vector.call_args
    assert args[0] == metadata["face_id"] # point_id should be the faceId
    assert args[1] == [0.1] * 128  # The dummy embedding
    assert args[2]["member_id"] == metadata["member_id"]
    assert args[2]["family_id"] == metadata["family_id"]
    assert result["face_id"] == metadata["face_id"]
    assert result["embedding"] == [0.1] * 128
    assert result["metadata"]["member_id"] == metadata["member_id"]

@pytest.mark.asyncio
async def test_add_face_missing_metadata(face_manager_instance, dummy_image):
    """
    Kiểm tra add_face khi thiếu memberId hoặc familyId trong metadata.
    """
    with pytest.raises(ValueError, match="Metadata phải chứa 'member_id' và 'family_id'."):
        await face_manager_instance.add_face(dummy_image, {"localDbId": "local123"})
@pytest.mark.asyncio
async def test_get_faces_by_family_id(face_manager_instance, mock_qdrant_repository):
    """
    Kiểm tra chức năng get_faces_by_family_id.
    """
    family_id = "familyabc"
    mock_qdrant_repository.get_faces_by_family_id.return_value = [
        {"id": "face1", "payload": {"familyId": family_id, "memberId": "mem1"}},
        {"id": "face2", "payload": {"familyId": family_id, "memberId": "mem2"}}
    ]
    
    faces = await face_manager_instance.get_faces_by_family_id(family_id)
    
    mock_qdrant_repository.get_faces_by_family_id.assert_called_once_with(family_id)
    assert len(faces) == 2
    assert faces[0]["id"] == "face1"
    assert faces[1]["payload"]["memberId"] == "mem2"

@pytest.mark.asyncio
async def test_delete_face_success(face_manager_instance, mock_qdrant_repository):
    """
    Kiểm tra chức năng delete_face thành công.
    """
    face_id = "face_to_delete"
    mock_qdrant_repository.delete_face.return_value = True
    
    success = await face_manager_instance.delete_face(face_id)
    
    mock_qdrant_repository.delete_face.assert_called_once_with(face_id)
    assert success is True

@pytest.mark.asyncio
async def test_delete_face_failure(face_manager_instance, mock_qdrant_repository):
    """
    Kiểm tra chức năng delete_face thất bại.
    """
    face_id = "face_to_delete_fail"
    mock_qdrant_repository.delete_face.return_value = False
    
    success = await face_manager_instance.delete_face(face_id)
    
    mock_qdrant_repository.delete_face.assert_called_once_with(face_id)
    assert success is False

@pytest.mark.asyncio
async def test_search_similar_faces(face_manager_instance, mock_qdrant_repository, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng search_similar_faces.
    """
    family_id = "family_search"
    limit = 3
    query_embedding = [0.1] * 128
    mock_face_embedding_service.get_embedding.return_value = query_embedding
    
    mock_qdrant_repository.search_similar_faces.return_value = [
        {"id": "res1", "score": 0.99, "payload": {"familyId": family_id}},
        {"id": "res2", "score": 0.98, "payload": {"familyId": family_id}},
    ]
    
    results = await face_manager_instance.search_similar_faces(dummy_image, family_id, limit)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_repository.search_similar_faces.assert_called_once_with(
        query_embedding,
        family_id=family_id,
        top_k=limit,
        threshold=0.7 # Default threshold
    )
    assert len(results) == 2
    assert results[0]["id"] == "res1"
    assert results[1]["score"] == 0.98

@pytest.mark.asyncio
async def test_search_similar_faces_no_family_id(face_manager_instance, mock_qdrant_repository, mock_face_embedding_service, dummy_image):
    """
    Kiểm tra chức năng search_similar_faces khi không có family_id.
    """
    limit = 5
    query_embedding = [0.1] * 128
    mock_face_embedding_service.get_embedding.return_value = query_embedding
    
    mock_qdrant_repository.search_similar_faces.return_value = [
        {"id": "res1", "score": 0.99, "payload": {}},
    ]
    
    results = await face_manager_instance.search_similar_faces(dummy_image, None, limit)
    
    mock_face_embedding_service.get_embedding.assert_called_once_with(dummy_image)
    mock_qdrant_repository.search_similar_faces.assert_called_once_with(
        query_embedding,
        family_id=None,
        top_k=limit,
        threshold=0.7 # Default threshold
    )
    assert len(results) == 1
    assert results[0]["id"] == "res1"

@pytest.mark.asyncio
async def test_add_face_by_vector_success(face_manager_instance, mock_qdrant_repository):
    """
    Kiểm tra chức năng add_face_by_vector thành công.
    """
    vector = [0.2] * 128
    metadata = {
        "member_id": "member456",
        "family_id": "familyxyz",
        "localDbId": "local456",
        "thumbnailUrl": "http://example.com/thumb2.png",
        "originalImageUrl": "http://example.com/orig2.jpg",
        "emotion": "sad",
        "emotionConfidence": 0.7,
        "face_id": "face456"
    }
    result = await face_manager_instance.add_face_by_vector(vector, metadata)

    mock_qdrant_repository.upsert_face_vector.assert_called_once()
    
    args, kwargs = mock_qdrant_repository.upsert_face_vector.call_args
    assert args[0] == metadata["face_id"]
    assert args[1] == vector
    assert args[2]["member_id"] == metadata["member_id"]
    assert args[2]["family_id"] == metadata["family_id"]

    assert result["face_id"] == metadata["face_id"]
    assert result["embedding"] == vector
    assert result["metadata"]["member_id"] == metadata["member_id"]

@pytest.mark.asyncio
async def test_add_face_by_vector_missing_metadata(face_manager_instance):
    """
    Kiểm tra add_face_by_vector khi thiếu memberId hoặc familyId trong metadata.
    """
    vector = [0.2] * 128
    with pytest.raises(ValueError, match="Metadata phải chứa 'member_id' và 'family_id'."):
        await face_manager_instance.add_face_by_vector(vector, {"localDbId": "local456"})