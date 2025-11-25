import pytest
import sys
import os

# Assuming get_emotion.py is in the parent directory
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from get_emotion import get_emotion

def test_get_emotion_placeholder_returns_valid_data():
    # Dummy base64 for testing
    dummy_base64_image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="
    emotion, confidence = get_emotion(dummy_base64_image)
    
    assert isinstance(emotion, str)
    assert emotion in ["happy", "sad", "neutral", "angry", "surprise", "fear", "disgust", "unknown"]
    assert isinstance(confidence, float)
    assert 0.0 <= confidence <= 1.0

def test_get_emotion_handles_invalid_base64_gracefully():
    invalid_base64 = "not-a-valid-base64-string"
    emotion, confidence = get_emotion(invalid_base64)
    
    assert emotion == "unknown"
    assert confidence == 0.0

