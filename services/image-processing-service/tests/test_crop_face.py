import pytest
import io
import base64
from PIL import Image
import cv2
import numpy as np

# Assuming crop_face.py is in the parent directory
import sys
import os
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from crop_face import crop_and_resize_face, load_image_from_bytes, encode_image_to_base64

def test_crop_and_resize_face_valid_input(image_with_face_bytes, dummy_face_location):
    # Ensure the dummy_face_location makes sense for image_with_face_bytes
    # image_with_face_bytes is 100x100, face is ~30,30,70,70.
    # dummy_face_location (10, 40, 50, 20) is within these bounds.
    cropped_base64 = crop_and_resize_face(image_with_face_bytes, dummy_face_location)
    assert cropped_base64 is not None
    assert isinstance(cropped_base64, str)

    # Decode and check dimensions
    cropped_image_bytes = base64.b64decode(cropped_base64)
    nparr = np.frombuffer(cropped_image_bytes, np.uint8)
    cropped_image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    assert cropped_image is not None
    assert cropped_image.shape == (512, 512, 3) # Target size 512x512 with 3 channels

def test_crop_and_resize_face_invalid_image_bytes(invalid_image_bytes, dummy_face_location):
    cropped_base64 = crop_and_resize_face(invalid_image_bytes, dummy_face_location)
    assert cropped_base64 is None # Expect None on error

def test_crop_and_resize_face_out_of_bounds_location(image_with_face_bytes):
    # Location completely out of bounds
    out_of_bounds_location = (500, 600, 550, 500)
    cropped_base64 = crop_and_resize_face(image_with_face_bytes, out_of_bounds_location)
    assert cropped_base64 is None # Expect None as the crop is out of bounds and should return None

def test_crop_and_resize_face_edge_location(image_with_face_bytes):
    # Location at the very edge of the image
    edge_location = (0, 100, 100, 0) # Full image
    cropped_base64 = crop_and_resize_face(image_with_face_bytes, edge_location)
    assert cropped_base64 is not None
    
    cropped_image_bytes = base64.b64decode(cropped_base64)
    nparr = np.frombuffer(cropped_image_bytes, np.uint8)
    cropped_image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    assert cropped_image.shape == (512, 512, 3)

# Test load_image_from_bytes
def test_load_image_from_bytes_valid(image_with_face_bytes):
    img = load_image_from_bytes(image_with_face_bytes)
    assert img is not None
    assert isinstance(img, np.ndarray)

def test_load_image_from_bytes_invalid():
    img = load_image_from_bytes(b"invalid image data")
    assert img is None

# Test encode_image_to_base64
def test_encode_image_to_base64_valid():
    dummy_img = np.zeros((10, 10, 3), dtype=np.uint8)
    base64_str = encode_image_to_base64(dummy_img)
    assert isinstance(base64_str, str)
    assert len(base64_str) > 0
