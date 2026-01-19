import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock
from app.main import app
from uuid import uuid4
from app.config import EMBEDDING_DIMENSIONS

client = TestClient(app)

# Mock LanceDBService globally for API tests
@pytest.fixture(autouse=True)
def mock_lancedb_service():
    with patch('app.api.faces.lancedb_service', autospec=True) as mock_service:
        yield mock_service

# Mock embedding_service globally for API tests
@pytest.fixture(autouse=True)
def mock_embedding_service():
    with patch('app.api.faces.embedding_service', autospec=True) as mock_embed:
        mock_embed.embed_query.return_value = [0.1] * EMBEDDING_DIMENSIONS
        yield mock_embed


def test_add_face_success(mock_lancedb_service):
    family_id = str(uuid4())
    member_id = str(uuid4())
    
    face_data = {
        "face_metadata": {
            "family_id": family_id,
            "member_id": member_id,
            "embedding": [0.1] * EMBEDDING_DIMENSIONS,
            "confidence": 0.98
        }
    }
    
    mock_lancedb_service.add_face_data.return_value = None # add_face_data is async, returns None
    
    response = client.post("/api/v1/faces", json=face_data)
    
    assert response.status_code == 201
    assert response.json()["message"] == "Thông tin khuôn mặt đã được thêm thành công."
    mock_lancedb_service.add_face_data.assert_called_once()
    # Check if face_id is a valid UUID
    assert isinstance(uuid4(), type(uuid4(response.json()["face_id"])))


def test_add_face_missing_embedding_and_image_url():
    family_id = str(uuid4())
    member_id = str(uuid4())

    face_data = {
        "face_metadata": {
            "family_id": family_id,
            "member_id": member_id,
            "confidence": 0.98
            # Missing embedding and original_image_url
        }
    }
    
    response = client.post("/api/v1/faces", json=face_data)
    
    assert response.status_code == 400
    assert "Cần cung cấp 'embedding' hoặc 'original_image_url' để tạo nhúng." in response.json()["detail"]


def test_add_face_image_url_embedding_not_implemented():
    family_id = str(uuid4())
    member_id = str(uuid4())

    face_data = {
        "face_metadata": {
            "family_id": family_id,
            "member_id": member_id,
            "original_image_url": "http://example.com/image.jpg",
            "confidence": 0.98
        }
    }
    
    response = client.post("/api/v1/faces", json=face_data)
    
    assert response.status_code == 501
    assert "Tạo nhúng từ URL hình ảnh chưa được triển khai." in response.json()["detail"]


def test_search_faces_success(mock_lancedb_service):
    family_id = str(uuid4())
    member_id = str(uuid4())
    face_id = str(uuid4())
    
    mock_lancedb_service.search_faces.return_value = [
        {"face_id": face_id, "member_id": member_id, "_distance": 0.05}
    ]
    
    search_request = {
        "query_embedding": [0.1] * EMBEDDING_DIMENSIONS,
        "family_id": family_id,
        "top_k": 1
    }
    
    response = client.post("/api/v1/faces/search", json=search_request)
    
    assert response.status_code == 200
    results = response.json()
    assert len(results) == 1
    assert results[0]["face_id"] == face_id
    assert results[0]["member_id"] == member_id
    assert results[0]["score"] == 0.05
    mock_lancedb_service.search_faces.assert_called_once_with(
        family_id=family_id,
        query_embedding=search_request["query_embedding"],
        member_id=None,
        top_k=1
    )


def test_search_faces_missing_embedding_and_image_url():
    family_id = str(uuid4())
    search_request = {
        "family_id": family_id,
        "top_k": 1
        # Missing query_embedding and image_url
    }
    
    response = client.post("/api/v1/faces/search", json=search_request)
    
    assert response.status_code == 400
    assert "Cần cung cấp 'query_embedding' hoặc 'image_url' để tìm kiếm." in response.json()["detail"]


def test_search_faces_image_url_embedding_not_implemented():
    family_id = str(uuid4())
    search_request = {
        "family_id": family_id,
        "image_url": "http://example.com/search_image.jpg",
        "top_k": 1
    }
    
    response = client.post("/api/v1/faces/search", json=search_request)
    
    assert response.status_code == 501
    assert "Tạo nhúng từ URL hình ảnh để tìm kiếm chưa được triển khai." in response.json()["detail"]


def test_delete_face_success(mock_lancedb_service):
    family_id = str(uuid4())
    face_id = str(uuid4())
    
    mock_lancedb_service.delete_face_data.return_value = 1 # 1 row deleted
    
    response = client.delete(f"/api/v1/faces/{family_id}/{face_id}")
    
    assert response.status_code == 200
    assert response.json()["message"] == f"Khuôn mặt với face_id '{face_id}' đã được xóa thành công."
    mock_lancedb_service.delete_face_data.assert_called_once_with(
        family_id=family_id,
        face_id=face_id
    )


def test_delete_face_not_found(mock_lancedb_service):
    family_id = str(uuid4())
    face_id = str(uuid4())
    
    mock_lancedb_service.delete_face_data.return_value = 0 # 0 rows deleted
    
    response = client.delete(f"/api/v1/faces/{family_id}/{face_id}")
    
    assert response.status_code == 404
    assert f"Không tìm thấy khuôn mặt với face_id '{face_id}' trong family_id '{family_id}' để xóa." in response.json()["detail"]


def test_delete_faces_by_member_id_success(mock_lancedb_service):
    family_id = str(uuid4())
    member_id = str(uuid4())
    
    mock_lancedb_service.delete_face_data.return_value = 2 # 2 rows deleted
    
    response = client.delete(f"/api/v1/faces/member/{family_id}/{member_id}")
    
    assert response.status_code == 200
    assert response.json()["message"] == f"Tất cả khuôn mặt của thành viên '{member_id}' trong family_id '{family_id}' đã được xóa thành công."
    mock_lancedb_service.delete_face_data.assert_called_once_with(
        family_id=family_id,
        member_id=member_id
    )


def test_delete_faces_by_member_id_not_found(mock_lancedb_service):
    family_id = str(uuid4())
    member_id = str(uuid4())
    
    mock_lancedb_service.delete_face_data.return_value = 0 # 0 rows deleted
    
    response = client.delete(f"/api/v1/faces/member/{family_id}/{member_id}")
    
    assert response.status_code == 404
    assert f"Không tìm thấy khuôn mặt nào cho member_id '{member_id}' trong family_id '{family_id}' để xóa." in response.json()["detail"]


def test_update_face_success(mock_lancedb_service):
    family_id = str(uuid4())
    face_id = str(uuid4())
    member_id = str(uuid4())
    
    mock_lancedb_service.update_face_data.return_value = 1 # 1 row updated
    
    update_data = {
        "confidence": 0.99,
        "emotion": "surprised",
        "member_id": member_id # Included to satisfy Pydantic, but should be ignored in actual update if it's part of the path
    }
    
    response = client.put(f"/api/v1/faces/{family_id}/{face_id}", json=update_data)
    
    assert response.status_code == 200
    assert response.json()["message"] == f"Thông tin khuôn mặt với face_id '{face_id}' đã được cập nhật thành công."
    mock_lancedb_service.update_face_data.assert_called_once()
    args, kwargs = mock_lancedb_service.update_face_data.call_args
    assert kwargs["family_id"] == family_id
    assert kwargs["face_id"] == face_id
    assert kwargs["update_data"]["confidence"] == 0.99
    assert kwargs["update_data"]["emotion"] == "surprised"
    assert "member_id" in kwargs["update_data"] # It will be in update_data, but api removes face_id.

def test_update_face_not_found(mock_lancedb_service):
    family_id = str(uuid4())
    face_id = str(uuid4())
    member_id = str(uuid4())
    
    mock_lancedb_service.update_face_data.return_value = 0 # 0 rows updated
    
    update_data = {
        "confidence": 0.99,
        "member_id": member_id
    }
    
    response = client.put(f"/api/v1/faces/{family_id}/{face_id}", json=update_data)
    
    assert response.status_code == 404
    assert f"Không tìm thấy khuôn mặt với face_id '{face_id}' trong family_id '{family_id}' để cập nhật." in response.json()["detail"]
