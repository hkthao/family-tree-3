import unittest
from unittest.mock import MagicMock, patch
import numpy as np

# Import the class to be tested
from src.infrastructure.detectors.retinaface_detector import RetinaFaceDetector
from src.domain.interfaces.face_detector import IFaceDetector

class TestRetinaFaceDetector(unittest.TestCase):

    @patch('src.infrastructure.detectors.retinaface_detector.FaceAnalysis')
    def setUp(self, MockFaceAnalysis):
        # Setup run before each test method
        self.mock_app_instance = MockFaceAnalysis.return_value
        self.face_detector = RetinaFaceDetector()
        
        # Ensure prepare is called during initialization
        self.mock_app_instance.prepare.assert_called_once_with(ctx_id=0, det_size=(640, 640))

        # Create a dummy image for testing
        self.dummy_image = np.zeros((100, 100, 3), dtype=np.uint8)

    def test_detect_faces_no_face(self):
        # Test case when no faces are detected
        self.mock_app_instance.get.return_value = []
        detected_faces = self.face_detector.detect_faces(self.dummy_image)
        self.assertEqual(detected_faces, [])
        self.mock_app_instance.get.assert_called_once_with(self.dummy_image)

    def test_detect_faces_one_face(self):
        # Test case when one face is detected
        mock_face = MagicMock()
        mock_face.bbox = np.array([10, 20, 40, 60]) # x1, y1, x2, y2
        mock_face.det_score = 0.95
        self.mock_app_instance.get.return_value = [mock_face]

        detected_faces = self.face_detector.detect_faces(self.dummy_image)

        self.assertEqual(len(detected_faces), 1)
        # Expected box format: [x, y, w, h]
        self.assertEqual(detected_faces[0]['box'], [10, 20, 30, 40])
        self.assertEqual(detected_faces[0]['confidence'], 0.95)
        self.mock_app_instance.get.assert_called_once_with(self.dummy_image)

    def test_detect_faces_multiple_faces(self):
        # Test case when multiple faces are detected
        mock_face1 = MagicMock()
        mock_face1.bbox = np.array([10, 10, 50, 50]) # x1, y1, x2, y2 -> x,y,w,h = 10,10,40,40
        mock_face1.det_score = 0.98

        mock_face2 = MagicMock()
        mock_face2.bbox = np.array([60, 60, 90, 90]) # x1, y1, x2, y2 -> x,y,w,h = 60,60,30,30
        mock_face2.det_score = 0.92

        self.mock_app_instance.get.return_value = [mock_face1, mock_face2]

        detected_faces = self.face_detector.detect_faces(self.dummy_image)

        self.assertEqual(len(detected_faces), 2)
        self.assertEqual(detected_faces[0]['box'], [10, 10, 40, 40])
        self.assertEqual(detected_faces[0]['confidence'], 0.98)
        self.assertEqual(detected_faces[1]['box'], [60, 60, 30, 30])
        self.assertEqual(detected_faces[1]['confidence'], 0.92)
        self.mock_app_instance.get.assert_called_once_with(self.dummy_image)

    def test_detect_faces_insightface_error(self):
        # Test case for when insightface.app.FaceAnalysis.get raises an exception
        self.mock_app_instance.get.side_effect = Exception("InsightFace error")
        with self.assertRaises(Exception):
            self.face_detector.detect_faces(self.dummy_image)
        self.mock_app_instance.get.assert_called_once_with(self.dummy_image)

