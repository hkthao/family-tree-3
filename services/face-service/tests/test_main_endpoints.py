import os
import pytest
from fastapi.testclient import TestClient
from unittest.mock import Mock, patch
import io
import base64
import json
from PIL import Image
import uuid

# Import the main FastAPI app and services for mocking
# The actual import of 'app' will happen inside the 'client' fixture
# from app.main import app 
from app.services.qdrant_service import QdrantService
from app.services.face_embedding import FaceEmbeddingService
from app.services.face_detector import DlibFaceDetector
from app.services.face_service import FaceService

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
        # Patch the classes directly in app.services, as app.main imports them from there.
        # We patch at the source where they are defined.
        with patch('app.services.qdrant_service.QdrantService') as MockQdrantService, \
             patch('app.services.face_embedding.FaceEmbeddingService') as MockFaceEmbeddingService, \
             patch('app.services.face_detector.DlibFaceDetector') as MockDlibFaceDetector:
                    
            # Mock QdrantService instance
            mock_qdrant_instance = MockQdrantService.return_value
            mock_qdrant_instance.upsert_face_embedding.return_value = None
            mock_qdrant_instance.get_points_by_payload_filter.return_value = []
            mock_qdrant_instance.delete_point_by_id.return_value = True
            mock_qdrant_instance.search_face_embeddings.return_value = []

            # Mock FaceEmbeddingService instance
            mock_face_embedding_instance = MockFaceEmbeddingService.return_value
            mock_face_embedding_instance.get_embedding.return_value = [0.1] * 128

            # Mock DlibFaceDetector instance
            mock_face_detector_instance = MockDlibFaceDetector.return_value
            mock_face_detector_instance.detect_faces.return_value = [
                {'box': (10, 10, 50, 50), 'confidence': 0.99}
            ]
            yield {
                "qdrant_service": mock_qdrant_instance,
                "face_embedding_service": mock_face_embedding_instance,
                "face_detector": mock_face_detector_instance
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
    # Re-set return_value for face_detector.detect_faces as it's cleared by reset_mock()
    mock_all_services_session_scope["face_detector"].detect_faces.return_value = [
        {'box': (10, 10, 50, 50), 'confidence': 0.99}
    ]
    # Re-set return_value for face_embedding_service.get_embedding as it's cleared by reset_mock()
    mock_all_services_session_scope["face_embedding_service"].get_embedding.return_value = [0.1] * 128
    yield


@pytest.fixture
def client(mock_all_services_session_scope):
    """
    Fixture for TestClient, ensuring app is imported after services are mocked.
    """
    # Import app here so that the service initializations in app.main
    # use the mocked versions of the services.
    from app.main import app as fastapi_app
    # Re-initialize the global service instances in app.main using the mocked dependencies
    # This is crucial because app.main initializes its services at module level.
    with patch('app.main.qdrant_service', new=mock_all_services_session_scope["qdrant_service"]), \
         patch('app.main.face_embedding_service', new=mock_all_services_session_scope["face_embedding_service"]), \
         patch('app.main.face_detector', new=mock_all_services_session_scope["face_detector"]), \
         patch('app.main.face_service.qdrant_service', new=mock_all_services_session_scope["qdrant_service"]), \
         patch('app.main.face_service.face_embedding_service', new=mock_all_services_session_scope["face_embedding_service"]):
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
        "localDbId": "aa1272a1-ebdc-4b23-a49b-f108c3c8ab64",
        "memberId": "ca107c9a-d013-46cb-8a33-552c8b7d385a",
        "familyId": "e4757d91-509b-4ac0-8807-8d0b82e3b7ec",
        "thumbnailUrl": "https://res.cloudinary.com/dcdjaqq41/image/upload/v1767275869/dong-ho-viet-prod/dong-ho-viet-prod_21c97af0-50b6-43aa-9377-eb456c8c4fcc.png",
        "originalImageUrl": "https://res.cloudinary.com/dcdjaqq41/image/upload/v1767275867/dong-ho-viet-prod/dong-ho-viet-prod_a821a49a-a20e-45c7-ab0a-c99e4368143a.jpg",
        "emotion": "",
        "emotionConfidence": 0
    }

def test_add_face_endpoint(client, dummy_image_bytes, dummy_metadata, mock_all_services_session_scope):
    """
    Test POST /faces endpoint for adding a new face.
    """
    dummy_metadata["faceId"] = str(uuid.uuid4())
    response = client.post(
        "/faces",
        files={"file": ("test.png", dummy_image_bytes, "image/png")},
        data={"metadata": json.dumps(dummy_metadata)}
    )
    assert response.status_code == 200
    assert "faceId" in response.json()
    assert response.json()["embedding"] == [0.1] * 128
    mock_all_services_session_scope["qdrant_service"].upsert_face_embedding.assert_called_once()
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_called_once()

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
    dummy_metadata["faceId"] = str(uuid.uuid4())
    payload = {
        "vector": vector_data,
        "metadata": dummy_metadata
    }
    response = client.post(
        "/faces/vector",
        json=payload
    )
    assert response.status_code == 200
    assert "faceId" in response.json()
    assert response.json()["embedding"] == vector_data
    mock_all_services_session_scope["qdrant_service"].upsert_face_embedding.assert_called_once()
    # FaceEmbeddingService.get_embedding should not be called for this endpoint
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_not_called()


def test_get_faces_by_family_endpoint(client, mock_all_services_session_scope):
    """
    Test GET /faces/family/{family_id} endpoint.
    """
    family_id = "e4757d91-509b-4ac0-8807-8d0b82e3b7ec"
    mock_all_services_session_scope["qdrant_service"].get_points_by_payload_filter.return_value = [
        {"id": "face1", "payload": {"familyId": family_id, "memberId": "member1"}}
    ]
    response = client.get(f"/faces/family/{family_id}")
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert response.json()[0]["id"] == "face1"
    mock_all_services_session_scope["qdrant_service"].get_points_by_payload_filter.assert_called_once_with(
        payload_filter={"familyId": family_id}
    )

def test_delete_face_endpoint(client, mock_all_services_session_scope):
    """
    Test DELETE /faces/{face_id} endpoint.
    """
    face_id = "test_face_id_to_delete"
    mock_all_services_session_scope["qdrant_service"].delete_point_by_id.return_value = True
    response = client.delete(f"/faces/{face_id}")
    assert response.status_code == 200
    assert "deleted successfully" in response.json()["message"]
    mock_all_services_session_scope["qdrant_service"].delete_point_by_id.assert_called_once_with(face_id)

def test_delete_face_endpoint_not_found(client, mock_all_services_session_scope):
    """
    Test DELETE /faces/{face_id} endpoint when face not found.
    """
    face_id = "non_existent_face"
    mock_all_services_session_scope["qdrant_service"].delete_point_by_id.return_value = False
    response = client.delete(f"/faces/{face_id}")
    assert response.status_code == 404
    assert "not found or could not be deleted" in response.json()["detail"]
    mock_all_services_session_scope["qdrant_service"].delete_point_by_id.assert_called_once_with(face_id)

def test_search_faces_endpoint(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /faces/search endpoint.
    """
    search_request_payload = {
        "query_image": base64.b64encode(dummy_image_bytes).decode("utf-8"),
        "familyId": "e4757d91-509b-4ac0-8807-8d0b82e3b7ec",
        "limit": 2
    }
    mock_all_services_session_scope["qdrant_service"].search_face_embeddings.return_value = [
        {"id": "search_res1", "score": 0.98, "payload": {}},
        {"id": "search_res2", "score": 0.97, "payload": {}},
    ]
    response = client.post("/faces/search", json=search_request_payload)
    assert response.status_code == 200
    assert len(response.json()) == 2
    assert response.json()[0]["id"] == "search_res1"
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_called_once()
    mock_all_services_session_scope["qdrant_service"].search_face_embeddings.assert_called_once()

def test_search_faces_endpoint_no_family_id(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /faces/search endpoint without familyId.
    """
    search_request_payload = {
        "query_image": base64.b64encode(dummy_image_bytes).decode("utf-8"),
        "limit": 1
    }
    mock_all_services_session_scope["qdrant_service"].search_face_embeddings.return_value = [
        {"id": "search_res_no_family", "score": 0.99, "payload": {}},
    ]
    response = client.post("/faces/search", json=search_request_payload)
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert response.json()[0]["id"] == "search_res_no_family"
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_called_once()
    mock_all_services_session_scope["qdrant_service"].search_face_embeddings.assert_called_once()

# Test the /detect endpoint again to ensure it's still working as expected
def test_detect_faces_endpoint(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint.
    """
    response = client.post(
        "/detect",
        files={"file": ("test.png", dummy_image_bytes, "image/png")}
    )
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert "id" in response.json()[0]
    assert "bounding_box" in response.json()[0]
    assert "embedding" in response.json()[0]
    mock_all_services_session_scope["face_detector"].detect_faces.assert_called_once()
    mock_all_services_session_scope["face_embedding_service"].get_embedding.assert_called_once()

def test_detect_faces_endpoint_no_faces(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint when no faces are detected.
    """
    mock_all_services_session_scope["face_detector"].detect_faces.return_value = []
    response = client.post(
        "/detect",
        files={"file": ("test.png", dummy_image_bytes, "image/png")}
    )
    assert response.status_code == 404
    assert "No faces detected" in response.json()["detail"]

def test_detect_faces_endpoint_return_crop(client, dummy_image_bytes, mock_all_services_session_scope):
    """
    Test POST /detect endpoint with return_crop=True.
    """
    response = client.post(
        "/detect?return_crop=true",
        files={"file": ("test.png", dummy_image_bytes, "image/png")}
    )
    assert response.status_code == 200
    assert len(response.json()) == 1
    assert "thumbnail" in response.json()[0]
    assert response.json()[0]["thumbnail"] is not None
    
# Test the /resize endpoint again to ensure it's still working as expected
def test_resize_image_endpoint(client, dummy_image_bytes):
    """
    Test POST /resize endpoint.
    """
    response = client.post(
        "/resize?width=50",
        files={"file": ("test.png", dummy_image_bytes, "image/png")}
    )
    assert response.status_code == 200
    assert response.headers["content-type"] == "image/png"
    # Further checks could involve loading the image and verifying its size
    resized_image = Image.open(io.BytesIO(response.content))
    assert resized_image.size == (50, 50)
