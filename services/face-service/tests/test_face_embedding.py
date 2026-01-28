import unittest
from unittest.mock import MagicMock, patch
import numpy as np
from PIL import Image


# Import the class to be tested
from app.services.face_embedding import FaceEmbeddingService


class TestFaceEmbeddingService(unittest.TestCase):

    @patch('app.services.face_embedding.dlib.shape_predictor')
    @patch('app.services.face_embedding.dlib.face_recognition_model_v1')
    def setUp(self, mock_face_recognition_model_v1, mock_shape_predictor):
        # Mock Dlib models
        self.mock_predictor = MagicMock()
        mock_shape_predictor.return_value = self.mock_predictor

        self.mock_face_encoder = MagicMock()
        mock_face_recognition_model_v1.return_value = self.mock_face_encoder

        self.face_embedding_service = FaceEmbeddingService()

        # Create a dummy PIL Image for testing
        self.dummy_pil_image = Image.new('RGB', (100, 100), color='red')

    @patch('app.services.face_embedding.cv2.cvtColor')
    @patch('app.services.face_embedding.dlib.rectangle')
    @patch('app.services.face_embedding.dlib.get_face_chip')
    def test_get_embedding_success(self, mock_get_face_chip, mock_dlib_rectangle, mock_cvtColor):
        # Mock the return values for Dlib functions
        mock_cvtColor.return_value = np.zeros((100, 100), dtype=np.uint8)  # Grayscale image
        mock_dlib_rectangle.return_value = MagicMock()  # A dlib.rectangle object
        
        # Mock landmarks
        mock_landmarks = MagicMock()
        self.mock_predictor.return_value = mock_landmarks # Mock the predictor to return landmarks

        mock_get_face_chip.return_value = np.zeros((150, 150, 3), dtype=np.uint8) # Aligned face

        # Mock the face encoder to return a dummy 128-dim embedding as a numpy array
        dummy_embedding = np.array([float(i) for i in range(128)], dtype=np.float64)
        self.mock_face_encoder.compute_face_descriptor.return_value = dummy_embedding

        embedding = self.face_embedding_service.get_embedding(self.dummy_pil_image)

        self.assertIsInstance(embedding, list)
        self.assertEqual(len(embedding), 128)
        self.assertTrue(all(isinstance(x, float) for x in embedding))

        # Verify mocks were called
        self.mock_predictor.assert_called_once()
        mock_dlib_rectangle.assert_called_once()
        mock_get_face_chip.assert_called_once()
        self.mock_face_encoder.compute_face_descriptor.assert_called_once()
        mock_cvtColor.assert_called_once()
    
    @patch('app.services.face_embedding.cv2.cvtColor', side_effect=Exception("CV2 Error"))
    def test_get_embedding_exception_handling(self, mock_cvtColor):
        # Test case for exception handling during embedding generation
        with self.assertRaises(Exception):
            self.face_embedding_service.get_embedding(self.dummy_pil_image)
