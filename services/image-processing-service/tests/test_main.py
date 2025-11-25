import pytest
from httpx import Client
import json
import io
from PIL import Image
import base64

# Fixture for client is in conftest.py
# Fixtures for image_with_face_bytes, image_no_face_bytes, invalid_image_bytes, large_image_bytes are in conftest.py

def test_read_root(client: Client):
    response = client.get("/")
    assert response.status_code == 200
    assert response.json() == {"message": "Image Processing Service is running!"}

def test_detect_faces_endpoint_with_face(client: Client, image_with_face_bytes: bytes):
    files = {"file": ("test_face.jpg", image_with_face_bytes, "image/jpeg")}
    response = client.post("/detect-faces/", files=files)
    assert response.status_code == 200
    data = response.json()
    assert data["filename"] == "test_face.jpg"
    assert isinstance(data["face_locations"], list)
    assert len(data["face_locations"]) == 0 # Expect 0 faces as face_recognition won't detect symbolic drawings


def test_detect_faces_endpoint_no_face(client: Client, image_no_face_bytes: bytes):
    files = {"file": ("test_no_face.jpg", image_no_face_bytes, "image/jpeg")}
    response = client.post("/detect-faces/", files=files)
    assert response.status_code == 200
    data = response.json()
    assert data["filename"] == "test_no_face.jpg"
    assert isinstance(data["face_locations"], list)
    assert len(data["face_locations"]) == 0

def test_detect_faces_endpoint_invalid_image(client: Client, invalid_image_bytes: bytes):
    files = {"file": ("test_invalid.jpg", invalid_image_bytes, "image/jpeg")}
    response = client.post("/detect-faces/", files=files)
    assert response.status_code == 400 # Expecting 400 Bad Request for invalid image data

def test_crop_and_analyze_face_endpoint_valid_input(client: Client, image_with_face_bytes: bytes):
    face_location = {"top": 30, "right": 70, "bottom": 70, "left": 30} # A reasonable face location
    
    files = {"file": ("test_face.jpg", image_with_face_bytes, "image/jpeg")}
    data = {
        "face_location_json": json.dumps(face_location),
        "member_id": "test_member_123"
    }
    response = client.post("/crop-and-analyze-face/", files=files, data=data)
    assert response.status_code == 200
    result = response.json()
    assert "cropped_face_base64" in result
    assert "emotion" in result
    assert "confidence" in result
    assert result["member_id"] == "test_member_123"
    
    # Check if cropped_face_base64 is a valid image (optional, but good)
    cropped_image_bytes = base64.b64decode(result["cropped_face_base64"])
    img_buffer = io.BytesIO(cropped_image_bytes)
    img = Image.open(img_buffer)
    assert img.width == 512
    assert img.height == 512

def test_crop_and_analyze_face_endpoint_invalid_json(client: Client, image_with_face_bytes: bytes):
    files = {"file": ("test_face.jpg", image_with_face_bytes, "image/jpeg")}
    data = {
        "face_location_json": "not a valid json",
    }
    response = client.post("/crop-and-analyze-face/", files=files, data=data)
    assert response.status_code == 400
    assert "Invalid JSON format for face_location" in response.json()["detail"]

def test_crop_and_analyze_face_endpoint_no_face_location(client: Client, image_with_face_bytes: bytes):
    files = {"file": ("test_face.jpg", image_with_face_bytes, "image/jpeg")}
    # Missing face_location_json
    response = client.post("/crop-and-analyze-face/", files=files)
    assert response.status_code == 422 # Unprocessable Entity due to missing form field

def test_normalize_image_endpoint_valid_image(client: Client, large_image_bytes: bytes):
    files = {"file": ("test_large.jpg", large_image_bytes, "image/jpeg")}
    response = client.post("/normalize-image/", files=files)
    assert response.status_code == 200
    result = response.json()
    assert "normalized_image_base64" in result
    assert result["original_filename"] == "test_large.jpg"

    # Check if normalized_image_base64 is a valid 512x512 image
    normalized_image_bytes = base64.b64decode(result["normalized_image_base64"])
    img_buffer = io.BytesIO(normalized_image_bytes)
    img = Image.open(img_buffer)
    assert img.width == 512
    assert img.height == 512

def test_normalize_image_endpoint_invalid_image(client: Client, invalid_image_bytes: bytes):
    files = {"file": ("test_invalid.jpg", invalid_image_bytes, "image/jpeg")}
    response = client.post("/normalize-image/", files=files)
    assert response.status_code == 500 # Expecting server error for invalid image data
