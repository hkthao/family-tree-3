import unittest
from unittest.mock import MagicMock, patch
import numpy as np
from PIL import Image

# Import the class to be tested
from src.infrastructure.embeddings.arcface_embedding import ArcFaceEmbedding
from src.domain.interfaces.face_embedding import IFaceEmbedding

class TestArcFaceEmbedding(unittest.TestCase):

    @patch('src.infrastructure.embeddings.arcface_embedding.FaceAnalysis')
    def setUp(self, MockFaceAnalysis):
        # Setup run before each test method
        self.mock_app_instance = MockFaceAnalysis.return_value
        self.face_embedding_service = ArcFaceEmbedding()
        
        # Ensure prepare is called during initialization
        self.mock_app_instance.prepare.assert_called_once_with(ctx_id=0, det_size=(640, 640))

        # Create a dummy PIL Image for testing
        self.dummy_pil_image = Image.new('RGB', (100, 100), color='red')
        
        # Mock cv2.cvtColor since it's used internally
        self.patcher_cv2_cvtcolor = patch('src.infrastructure.embeddings.arcface_embedding.cv2.cvtColor')
        self.mock_cv2_cvtcolor = self.patcher_cv2_cvtcolor.start()
        self.mock_cv2_cvtcolor.return_value = np.zeros((100, 100, 3), dtype=np.uint8) # Return a dummy BGR image

    def tearDown(self):
        self.patcher_cv2_cvtcolor.stop()

    def test_get_embedding_success(self):
        # Test case when embedding is successfully generated
        dummy_embedding_array = np.random.rand(512).astype(np.float32) # ArcFace usually produces 512-dim embeddings
        mock_face = MagicMock()
        mock_face.embedding = dummy_embedding_array
        self.mock_app_instance.get.return_value = [mock_face]

        embedding = self.face_embedding_service.get_embedding(self.dummy_pil_image)

        self.assertIsInstance(embedding, list)
        self.assertEqual(len(embedding), 512) # Check for 512-dim embedding
        self.assertTrue(all(isinstance(x, float) for x in embedding))
        
        # Verify that get was called with a numpy array (BGR converted from PIL RGB)
        self.mock_app_instance.get.assert_called_once()
        args, kwargs = self.mock_app_instance.get.call_args
        self.assertIsInstance(args[0], np.ndarray)
        self.assertEqual(args[0].shape, (100, 100, 3)) # Check shape of the numpy image passed to get()

    def test_get_embedding_no_face(self):
        # Test case when no face is detected in the cropped image
        self.mock_app_instance.get.return_value = []

        embedding = self.face_embedding_service.get_embedding(self.dummy_pil_image)

        self.assertEqual(embedding, [])
        self.mock_app_instance.get.assert_called_once()

    def test_get_embedding_insightface_error(self):
        # Test case for when insightface.app.FaceAnalysis.get raises an exception
        self.mock_app_instance.get.side_effect = Exception("InsightFace embedding error")
        with self.assertRaises(Exception):
            self.face_embedding_service.get_embedding(self.dummy_pil_image)
        self.mock_app_instance.get.assert_called_once()

