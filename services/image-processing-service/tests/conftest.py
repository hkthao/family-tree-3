import pytest
from PIL import Image, ImageDraw
import io
import os
import numpy as np

@pytest.fixture(scope="module")
def image_with_face_bytes():
    """Generates a dummy image with a simple face-like rectangle for testing."""
    img = Image.new('RGB', (100, 100), color = 'white')
    d = ImageDraw.Draw(img)
    # Draw a simple "face"
    d.ellipse((30, 30, 70, 70), fill=(255, 0, 0), outline=(0, 0, 0)) # Red circle face
    d.ellipse((40, 40, 45, 45), fill=(0, 0, 255)) # Blue eye 1
    d.ellipse((55, 40, 60, 45), fill=(0, 0, 255)) # Blue eye 2
    d.line((45, 60, 55, 60), fill=(0, 0, 0)) # Mouth
    
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

@pytest.fixture(scope="module")
def image_with_multiple_faces_bytes():
    """Generates a dummy image with multiple face-like rectangles for testing."""
    img = Image.new('RGB', (200, 100), color='white')
    d = ImageDraw.Draw(img)

    # Face 1
    d.ellipse((10, 20, 50, 60), fill=(255, 0, 0), outline=(0, 0, 0))
    d.ellipse((20, 30, 25, 35), fill=(0, 0, 255))
    d.ellipse((35, 30, 40, 35), fill=(0, 0, 255))

    # Face 2
    d.ellipse((100, 20, 140, 60), fill=(0, 255, 0), outline=(0, 0, 0))
    d.ellipse((110, 30, 115, 35), fill=(0, 0, 255))
    d.ellipse((125, 30, 130, 35), fill=(0, 0, 255))
    
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

@pytest.fixture(scope="module")
def image_no_face_bytes():
    """Generates a dummy image with no face for testing."""
    img = Image.new('RGB', (100, 100), color = 'blue')
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

@pytest.fixture(scope="module")
def invalid_image_bytes():
    """Generates invalid image bytes for testing error handling."""
    return b"this is not a valid image"

@pytest.fixture(scope="module")
def large_image_bytes():
    """Generates a large image for testing normalization."""
    img = Image.new('RGB', (1200, 1200), color = 'yellow')
    buf = io.BytesIO()
    img.save(buf, format='JPEG')
    return buf.getvalue()

@pytest.fixture(scope="module")
def dummy_face_location():
    """A dummy face location (top, right, bottom, left) for testing cropping."""
    return (10, 40, 50, 20) # Example location for an image where a face is supposed to be

@pytest.fixture(scope="module")
def client():
    """FastAPI test client fixture."""
    # Ensure that `main.py` is in the correct path relative to tests.
    # In this case, `main.py` is in the parent directory of `tests`.
    import sys
    sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
    from main import app 
    from fastapi.testclient import TestClient
    with TestClient(app=app, base_url="http://test") as client:
        yield client
