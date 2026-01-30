import unittest
from unittest.mock import MagicMock, patch
import numpy as np
from PIL import Image
import cv2 # Cần import cv2 vì nó được mock

# Import the class to be tested
from src.infrastructure.embeddings.arcface_embedding import ArcFaceEmbedding
from src.domain.interfaces.face_embedding import IFaceEmbedding

class TestArcFaceEmbedding(unittest.TestCase):

    @patch('src.infrastructure.embeddings.arcface_embedding.onnxruntime.InferenceSession')
    @patch('src.infrastructure.embeddings.arcface_embedding.os.path.join')
    def setUp(self, MockOsPathJoin, MockInferenceSession):
        # Mock os.path.join to return a predictable path
        MockOsPathJoin.return_value = 'dummy/path/w600k_r50.onnx'

        # Mock the InferenceSession object
        self.mock_rec_session = MagicMock()
        MockInferenceSession.return_value = self.mock_rec_session

        # Mock get_inputs and get_outputs methods of the mock_rec_session
        self.mock_rec_session.get_inputs.return_value = [MagicMock(name='input_name')]
        self.mock_rec_session.get_outputs.return_value = [MagicMock(name='output_name')]

        # Instantiate ArcFaceEmbedding service
        self.face_embedding_service = ArcFaceEmbedding()

        # Ensure InferenceSession was called correctly
        MockInferenceSession.assert_called_once_with('dummy/path/w600k_r50.onnx', providers=['CPUExecutionProvider'])

        # Create a dummy PIL Image for testing
        self.dummy_pil_image = Image.new('RGB', (100, 100), color='red')
        
        # Mock cv2.cvtColor since it's used internally
        self.patcher_cv2_cvtcolor = patch('src.infrastructure.embeddings.arcface_embedding.cv2.cvtColor')
        self.mock_cv2_cvtcolor = self.patcher_cv2_cvtcolor.start()
        # Mock cvtColor to return a dummy BGR image (original size)
        self.mock_cv2_cvtcolor.return_value = np.zeros((100, 100, 3), dtype=np.uint8) 

        # Mock cv2.resize since it's used internally by _preprocess
        self.patcher_cv2_resize = patch('src.infrastructure.embeddings.arcface_embedding.cv2.resize')
        self.mock_cv2_resize = self.patcher_cv2_resize.start()
        # Mock cv2.resize to return a 112x112 BGR image
        self.mock_cv2_resize.return_value = np.zeros((112, 112, 3), dtype=np.uint8)

        # Mock np.transpose and np.expand_dims as they are used in _preprocess
        self.patcher_np_transpose = patch('src.infrastructure.embeddings.arcface_embedding.np.transpose')
        self.mock_np_transpose = self.patcher_np_transpose.start()
        self.mock_np_transpose.return_value = np.zeros((3, 112, 112), dtype=np.float32)

        self.patcher_np_expand_dims = patch('src.infrastructure.embeddings.arcface_embedding.np.expand_dims')
        self.mock_np_expand_dims = self.patcher_np_expand_dims.start()
        self.mock_np_expand_dims.return_value = np.zeros((1, 3, 112, 112), dtype=np.float32)

    def tearDown(self):
        self.patcher_cv2_cvtcolor.stop()
        self.patcher_cv2_resize.stop()
        self.patcher_np_transpose.stop()
        self.patcher_np_expand_dims.stop()

    def test_get_embedding_success(self):
        # Test case when embedding is successfully generated
        dummy_embedding_array = np.random.rand(512).astype(np.float32) # ONNX returns (512,)
        self.mock_rec_session.run.return_value = [dummy_embedding_array] # run returns a list

        embedding = self.face_embedding_service.get_embedding(self.dummy_pil_image)

        self.assertIsInstance(embedding, list)
        self.assertEqual(len(embedding), 512) # Check for 512-dim embedding
        self.assertTrue(all(isinstance(x, float) for x in embedding))
        
        # Verify that run() was called with a preprocessed numpy array
        self.mock_rec_session.run.assert_called_once()
        args, kwargs = self.mock_rec_session.run.call_args
        self.assertIsInstance(args[1][self.mock_rec_session.get_inputs.return_value[0].name], np.ndarray)
        self.assertEqual(args[1][self.mock_rec_session.get_inputs.return_value[0].name].shape, (1, 3, 112, 112)) # Check shape

    def test_get_embedding_empty_input_no_embedding(self):
        # Test case for when the model returns an empty embedding
        self.mock_rec_session.run.return_value = [np.array([])] # run returns a list with an empty array

        embedding = self.face_embedding_service.get_embedding(self.dummy_pil_image)

        self.assertEqual(embedding, [])
        self.mock_rec_session.run.assert_called_once()

    def test_get_embedding_model_raises_exception(self):
        # Test case for when the underlying model's run method raises an exception
        self.mock_rec_session.run.side_effect = Exception("ONNX Runtime error")
        with self.assertRaises(Exception):
            self.face_embedding_service.get_embedding(self.dummy_pil_image)
        self.mock_rec_session.run.assert_called_once()

