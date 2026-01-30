import os
import pytest
from fastapi.testclient import TestClient
from unittest.mock import Mock, patch, AsyncMock
import io
import base64
import json
from PIL import Image
import uuid

# Import the main FastAPI app and services for mocking
# The actual import of 'app' will happen inside the 'client' fixture
# from app.main import app 
from src.infrastructure.persistence.qdrant_client import QdrantFaceRepository
from src.infrastructure.embeddings.facenet_embedding import FaceNetEmbeddingService
from src.infrastructure.detectors.dlib_detector import DlibFaceDetector
from src.application.services.face_manager import FaceManager


@pytest.fixture(scope="session", autouse=True)
def mock_all_services_session_scope():
    """
    Mock all external service dependencies (QdrantService, FaceEmbeddingService, DlibFaceDetector)
    for the duration of the endpoint tests. This runs once per session.
    """
    # Temporarily set environment variables
    os.environ["QDRANT_HOST"] = "http://mock-qdrant:6333"
    os.environ["QDRANT_API_KEY"] = "mock-api-key"
    os.environ["QDRANT_COLLECTION_NAME"] = "test_collection_name"

    try:
        with patch('src.infrastructure.persistence.qdrant_client.QdrantClient') as MockActualQdrantClient, \
             patch('src.infrastructure.persistence.qdrant_client.QdrantFaceRepository') as MockQdrantFaceRepository, \
             patch('src.infrastructure.embeddings.facenet_embedding.FaceNetEmbeddingService') as MockFaceNetEmbeddingService, \
             patch('src.infrastructure.detectors.dlib_detector.DlibFaceDetector') as MockDlibFaceDetector:
            
            # Mock the QdrantClient class that QdrantFaceRepository uses
            mock_actual_qdrant_client_instance = MockActualQdrantClient.return_value
            mock_actual_qdrant_client_instance.collection_exists.return_value = True # Assume collection exists
            mock_actual_qdrant_client_instance.create_collection.return_value = None
            mock_actual_qdrant_client_instance.create_payload_index.return_value = None

            mock_qdrant_instance = MockQdrantFaceRepository.return_value
            mock_qdrant_instance.upsert_face_vector = AsyncMock(return_value=None)
            mock_qdrant_instance.get_faces_by_family_id = AsyncMock(return_value=[])
            mock_qdrant_instance.delete_face = AsyncMock(return_value=True)
            mock_qdrant_instance.delete_faces_by_family_id = AsyncMock(return_value=True)
            mock_qdrant_instance.search_similar_faces = AsyncMock(return_value=[])

            mock_face_embedding_instance = MockFaceNetEmbeddingService.return_value
            mock_face_embedding_instance.get_embedding = Mock(return_value=[0.1] * 128)

            mock_face_detector_instance = MockDlibFaceDetector.return_value
            mock_face_detector_instance.detect_faces = Mock(return_value=[
                {'box': (10, 10, 50, 50), 'confidence': 0.99}
            ])
            yield {
                "qdrant_repository": mock_qdrant_instance,
                "face_embedding_service": mock_face_embedding_instance,
                "face_detector": mock_face_detector_instance,
                "qdrant_client_mock": mock_actual_qdrant_client_instance # Use the new name
            }
    finally:
        # Clean up environment variables
        del os.environ["QDRANT_HOST"]
        del os.environ["QDRANT_API_KEY"]
        del os.environ["QDRANT_COLLECTION_NAME"]

@pytest.fixture(autouse=True)
def reset_mocks_for_each_test(mock_all_services_session_scope):
    """
    Resets the call count and other states of the mocks before each test function.
    """
    for service_mock in mock_all_services_session_scope.values():
        service_mock.reset_mock()
    mock_all_services_session_scope["face_detector"].detect_faces.return_value = [
        {'box': (10, 10, 50, 50), 'confidence': 0.99}
    ]
    mock_all_services_session_scope["face_embedding_service"].get_embedding.return_value = [0.1] * 128
    
    # Reset QdrantFaceRepository mocks
    mock_all_services_session_scope["qdrant_repository"].upsert_face_vector.reset_mock()
    mock_all_services_session_scope["qdrant_repository"].get_faces_by_family_id.reset_mock()
    mock_all_services_session_scope["qdrant_repository"].delete_face.reset_mock()
    mock_all_services_session_scope["qdrant_repository"].delete_faces_by_family_id.reset_mock()
    mock_all_services_session_scope["qdrant_repository"].search_similar_faces.reset_mock()
    
    mock_all_services_session_scope["qdrant_repository"].upsert_face_vector.return_value = None
    mock_all_services_session_scope["qdrant_repository"].get_faces_by_family_id.return_value = []
    mock_all_services_session_scope["qdrant_repository"].delete_face.return_value = True
    mock_all_services_session_scope["qdrant_repository"].delete_faces_by_family_id.return_value = True
    mock_all_services_session_scope["qdrant_repository"].search_similar_faces.return_value = []
    
    # Reset QdrantClient mock methods as well
    mock_all_services_session_scope["qdrant_client_mock"].collection_exists.reset_mock()
    mock_all_services_session_scope["qdrant_client_mock"].create_collection.reset_mock()
    mock_all_services_session_scope["qdrant_client_mock"].create_payload_index.reset_mock()
    
    mock_all_services_session_scope["qdrant_client_mock"].collection_exists.return_value = True # Assume collection exists
    mock_all_services_session_scope["qdrant_client_mock"].create_collection.return_value = None
    mock_all_services_session_scope["qdrant_client_mock"].create_payload_index.return_value = None
    yield


@pytest.fixture
def client(mock_all_services_session_scope):
    """
    Fixture for TestClient, ensuring app is imported after services are mocked.
    """
    # Import dependencies and app here, after mocks are set up
    from src.presentation.dependencies import get_face_detector, get_face_embedding_service, get_face_repository, get_face_manager
    from src.presentation.main import app as fastapi_app
    
    # Mock FaceManager separately for cases where its internal dependencies are used directly
    mock_face_manager_instance = AsyncMock(spec=FaceManager)
    mock_face_manager_instance.add_face = AsyncMock(side_effect=lambda face_image, metadata: {"face_id": metadata["face_id"], "embedding": [0.1]*128, "metadata": metadata})
    mock_face_manager_instance.add_face_by_vector = AsyncMock(side_effect=lambda vector, metadata: {"face_id": metadata["face_id"], "embedding": vector, "metadata": metadata})
    mock_face_manager_instance.get_faces_by_family_id = AsyncMock(return_value=[])
    mock_face_manager_instance.delete_face = AsyncMock(return_value=True)
    mock_face_manager_instance.delete_faces_by_family_id = AsyncMock(return_value=True)
    mock_face_manager_instance.search_similar_faces = AsyncMock(return_value=[])
    mock_face_manager_instance.search_similar_faces_by_vector = AsyncMock(return_value=[])
    mock_face_manager_instance.face_embedding_service = mock_all_services_session_scope["face_embedding_service"] # For /detect endpoint

    with patch('src.presentation.dependencies.get_face_detector', return_value=mock_all_services_session_scope["face_detector"]), \
         patch('src.presentation.dependencies.get_face_embedding_service', return_value=mock_all_services_session_scope["face_embedding_service"]), \
         patch('src.presentation.dependencies.get_face_repository', return_value=mock_all_services_session_scope["qdrant_repository"]), \
         patch('src.presentation.dependencies.get_face_manager', return_value=mock_face_manager_instance): # Patch get_face_manager
        yield TestClient(fastapi_app)

@pytest.fixture
def dummy_image_bytes():
    """Returns dummy image bytes."""
    img = Image.new('RGB', (100, 100), color = 'red')
    byte_arr = io.BytesIO()
    img.save(byte_arr, format='PNG')
    return byte_arr.getvalue()

@pytest.fixture
def dummy_metadata():
    """
    Returns dummy metadata dictionary.
    """
    return {
        "local_db_id": "aa1272a1-ebdc-4b23-a49b-f108c3c8ab64",
        "member_id": "ca107c9a-d013-46cb-8a33-552c8b7d385a",
        "family_id": "e4757d91-509b-4ac0-8807-8d0b82e3b7ec",
        "thumbnail_url": "https://res.cloudinary.com/dcdjaqq41/image/upload/v1767275869/dong-ho-viet-prod/dong-ho-viet-prod_21c97af0-50b6-43aa-9377-eb456c8c4fcc.png",
        "original_image_url": "https://res.cloudinary.com/dcdjaqq41/image/upload/v1767275867/dong-ho-viet-prod/dong-ho-viet-prod_a821a49a-a20e-45c7-ab0a-c99e4368143a.jpg",
        "emotion": "",
        "emotion_confidence": 0
    }

def test_add_face_endpoint(client, dummy_image_bytes, dummy_metadata, mock_all_services_session_scope):
    """
    Test POST /faces endpoint for adding a new face.
    """
    dummy_metadata["face_id"] = str(uuid.uuid4())
    response = client.post(
        "/faces",
        files={"file": ("test.png", dummy_image_bytes, "image/png")},
        data={"metadata": json.dumps(dummy_metadata)}
    )
    assert response.status_code == 200
    assert "face_id" in response.json()
    mock_all_services_session_scope["qdrant_repository"].upsert_face_vector.assert_called_once()

def test_add_face_endpoint_invalid_file_type(client, dummy_metadata):
    """
    Test POST /faces endpoint with an invalid file type.
    """
    response = client.post(
        "/faces",
        files={"file": ("test.txt", b"some text", "text/plain")},
        data={"metadata": json.dumps(dummy_metadata)}
    )
    assert response.status_code == 400
    assert "Invalid file type" in response.json()["detail"]

def test_add_face_by_vector_endpoint(client, dummy_metadata, mock_all_services_session_scope):
    """
    Test POST /faces/vector endpoint for adding a new face by vector.
    """
    vector_data = [0.5] * 128
    dummy_metadata["face_id"] = str(uuid.uuid4())
    payload = {
        "vector": vector_data,
        "metadata": dummy_metadata
    }
    response = client.post(
        "/faces/vector",
        json=payload
    )
    assert response.status_code == 200
    assert "face_id" in response.json()
    assert response.json()["embedding"] == vector_data
    mock_all_services_session_scope["qdrant_repository"].upsert_face_vector.assert_called_once()
    # FaceEmbeddingService.get_embedding should not be called for this endpoint
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_not_called()


def test_get_faces_by_family_endpoint(client, mock_all_services_session_scope):
    """
    Test GET /faces/family/{family_id} endpoint.
    """
    family_id = "e4757d91-509b-4ac0-8807-8d0b82e3b7ec"
    mock_all_services_session_scope["qdrant_repository"].get_faces_by_family_id.return_value = [
        {"id": "face1", "payload": {"familyId": family_id, "memberId": "member1"}}
    ]
    response = client.get(f"/faces/family/{family_id}")
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert response.json()[0]["id"] == "face1"
    mock_all_services_session_scope["qdrant_repository"].get_faces_by_family_id.assert_called_once_with(family_id)

def test_delete_face_endpoint(client, mock_all_services_session_scope):
    """
    Test DELETE /faces/{face_id} endpoint.
    """
    face_id = "test_face_id_to_delete"
    mock_all_services_session_scope["qdrant_repository"].delete_face.return_value = True
    response = client.delete(f"/faces/{face_id}")
    assert response.status_code == 200
    mock_all_services_session_scope["qdrant_repository"].delete_face.assert_called_once_with(face_id)

def test_delete_face_endpoint_not_found(client, mock_all_services_session_scope):
    """
    Test DELETE /faces/{face_id} endpoint when face not found.
    """
    face_id = "non_existent_face"
    mock_all_services_session_scope["qdrant_repository"].delete_face.return_value = False
    response = client.delete(f"/faces/{face_id}")
    assert response.status_code == 404
    assert "not found or could not be deleted" in response.json()["detail"]
    mock_all_services_session_scope["qdrant_repository"].delete_face.assert_called_once_with(face_id)

def test_search_faces_endpoint(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /faces/search endpoint.
    """
    search_request_payload = {
        "query_image": base64.b64encode(dummy_image_bytes).decode("utf-8"),
        "family_id": "e4757d91-509b-4ac0-8807-8d0b82e3b7ec",
        "limit": 2
    }
    mock_all_services_session_scope["qdrant_repository"].search_similar_faces.return_value = [
        {"id": "search_res1", "score": 0.98, "payload": {}},
        {"id": "search_res2", "score": 0.97, "payload": {}},
    ]
    response = client.post("/faces/search", json=search_request_payload)
    assert response.status_code == 200
    assert len(response.json()) == 2
    mock_all_services_session_scope["qdrant_repository"].search_similar_faces.assert_called_once()

def test_search_faces_endpoint_no_family_id(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /faces/search endpoint without familyId.
    """
    search_request_payload = {
        "query_image": base64.b64encode(dummy_image_bytes).decode("utf-8"),
        "limit": 1
    }
    mock_all_services_session_scope["qdrant_repository"].search_similar_faces.return_value = [
        {"id": "search_res_no_family", "score": 0.99, "payload": {}},
    ]
    response = client.post("/faces/search", json=search_request_payload)
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert response.json()[0]["id"] == "search_res_no_family"

# Test the /detect endpoint again to ensure it's still working as expected
def test_detect_faces_endpoint(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint.
    """
    mock_detected_faces = [
        {
            'box': [10, 10, 40, 40], # x, y, w, h
            'confidence': 0.99,
            'embedding': [0.1] * 512 # ArcFace usually returns 512-dim embeddings
        }
    ]
    with patch('src.application.services.face_manager.FaceManager.detect_and_embed_faces', return_value=mock_detected_faces) as mock_detect_and_embed:
        response = client.post(
            "/detect",
            files={"file": ("test.png", dummy_image_bytes, "image/png")}
        )
        assert response.status_code == 200
        assert len(response.json()) == 1
        assert "id" in response.json()[0]
        assert "bounding_box" in response.json()[0]
        assert "embedding" in response.json()[0]
        mock_detect_and_embed.assert_called_once()
        # Ensure the original face_detector and face_embedding_service mocks are not called directly by the endpoint logic
        mock_all_services_session_scope["face_detector"].detect_faces.assert_not_called()
        mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_not_called()

def test_detect_faces_endpoint_no_faces(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint when no faces are detected.
    """
    with patch('src.application.services.face_manager.FaceManager.detect_and_embed_faces', return_value=[]) as mock_detect_and_embed:
        response = client.post(
            "/detect",
            files={"file": ("test.png", dummy_image_bytes, "image/png")}
        )
        assert response.status_code == 404
        assert "No faces detected" in response.json()["detail"]
        mock_detect_and_embed.assert_called_once()

def test_detect_faces_endpoint_return_crop(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint with return_crop=True.
    """
    mock_detected_faces = [
        {
            'box': [10, 10, 40, 40], # x, y, w, h
            'confidence': 0.99,
            'embedding': [0.1] * 512
        }
    ]
    with patch('src.application.services.face_manager.FaceManager.detect_and_embed_faces', return_value=mock_detected_faces) as mock_detect_and_embed:
        response = client.post(
            "/detect?return_crop=true",
            files={"file": ("test.png", dummy_image_bytes, "image/png")}
        )
        assert response.status_code == 200
        assert len(response.json()) == 1
        assert "thumbnail" in response.json()[0]
        assert response.json()[0]["thumbnail"] is not None
        mock_detect_and_embed.assert_called_once()

