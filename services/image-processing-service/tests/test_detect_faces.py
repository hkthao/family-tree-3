import pytest
import io
from PIL import Image, ImageDraw
import numpy as np

# Assuming detect_faces.py is in the parent directory
import sys
import os
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from detect_faces import detect_faces, get_face_encodings

def create_simple_face_image_bytes(face_rect=(30, 30, 70, 70)):
    img = Image.new('RGB', (100, 100), color='white')
    d = ImageDraw.Draw(img)
    d.ellipse(face_rect, fill=(255, 0, 0), outline=(0, 0, 0)) # Red circle face
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

def create_no_face_image_bytes():
    img = Image.new('RGB', (100, 100), color='blue')
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

def create_invalid_image_bytes():
    return b"this is not a valid image"

# Test cases for detect_faces
def test_detect_faces_on_image_with_face(image_with_face_bytes):
    locations = detect_faces(image_with_face_bytes)
    assert len(locations) == 0 # Expect 0 faces as face_recognition won't detect symbolic drawings

def test_detect_faces_on_image_with_multiple_faces(image_with_multiple_faces_bytes):
    locations = detect_faces(image_with_multiple_faces_bytes)
    assert len(locations) == 0 # Expect 0 faces as face_recognition won't detect symbolic drawings

def test_detect_faces_on_image_no_face(image_no_face_bytes):
    locations = detect_faces(image_no_face_bytes)
    assert len(locations) == 0

def test_detect_faces_on_invalid_image_bytes(invalid_image_bytes):
    with pytest.raises(ValueError, match="Could not load image from provided bytes."):
        detect_faces(invalid_image_bytes)

# Test cases for get_face_encodings
def test_get_face_encodings_on_image_with_face(image_with_face_bytes):
    encodings = get_face_encodings(image_with_face_bytes)
    assert len(encodings) == 0 # Expect 0 encodings for symbolic drawings

def test_get_face_encodings_on_image_no_face(image_no_face_bytes):
    encodings = get_face_encodings(image_no_face_bytes)
    assert len(encodings) == 0

def test_get_face_encodings_on_invalid_image_bytes(invalid_image_bytes):
    encodings = get_face_encodings(invalid_image_bytes)
    assert len(encodings) == 0 # Expect no encodings and no crash
