import unittest
from unittest.mock import MagicMock, patch
import numpy as np


# Import the class to be tested
from app.services.face_detector import DlibFaceDetector


class TestDlibFaceDetector(unittest.TestCase):

    @patch('app.services.face_detector.dlib.get_frontal_face_detector')
    def setUp(self, mock_get_frontal_face_detector):
        # Setup run before each test method
        self.mock_dlib_detector = MagicMock()
        mock_get_frontal_face_detector.return_value = self.mock_dlib_detector
        self.face_detector = DlibFaceDetector()

        # Create a dummy image for testing
        self.dummy_image = np.zeros((100, 100, 3), dtype=np.uint8)

    def test_detect_faces_no_face(self):
        # Test case when no faces are detected
        self.mock_dlib_detector.return_value = []
        detected_faces = self.face_detector.detect_faces(self.dummy_image)
        self.assertEqual(detected_faces, [])
        # Ensure dlib detector was called
        self.mock_dlib_detector.assert_called_once()

    def test_detect_faces_one_face(self):
        # Test case when one face is detected
        mock_rect = MagicMock()
        mock_rect.left.return_value = 10
        mock_rect.top.return_value = 20
        mock_rect.width.return_value = 30
        mock_rect.height.return_value = 40
        self.mock_dlib_detector.return_value = [mock_rect]

        detected_faces = self.face_detector.detect_faces(self.dummy_image)

        self.assertEqual(len(detected_faces), 1)
        self.assertEqual(detected_faces[0]['box'], [10, 20, 30, 40])
        self.assertEqual(detected_faces[0]['confidence'], 0.99)
        self.mock_dlib_detector.assert_called_once()

    def test_detect_faces_multiple_faces(self):
        # Test case when multiple faces are detected
        mock_rect1 = MagicMock()
        mock_rect1.left.return_value = 10
        mock_rect1.top.return_value = 20
        mock_rect1.width.return_value = 30
        mock_rect1.height.return_value = 40

        mock_rect2 = MagicMock()
        mock_rect2.left.return_value = 50
        mock_rect2.top.return_value = 60
        mock_rect2.width.return_value = 70
        mock_rect2.height.return_value = 80

        self.mock_dlib_detector.return_value = [mock_rect1, mock_rect2]

        detected_faces = self.face_detector.detect_faces(self.dummy_image)

        self.assertEqual(len(detected_faces), 2)
        self.assertEqual(detected_faces[0]['box'], [10, 20, 30, 40])
        self.assertEqual(detected_faces[0]['confidence'], 0.99)
        self.assertEqual(detected_faces[1]['box'], [50, 60, 70, 80])
        self.assertEqual(detected_faces[1]['confidence'], 0.99)
        self.mock_dlib_detector.assert_called_once()

    @patch('app.services.face_detector.cv2.cvtColor', side_effect=Exception("CV2 Error"))
    def test_detect_faces_exception_handling(self, mock_cvtColor):
        # Test case for exception handling during face detection
        with self.assertRaises(Exception):
            self.face_detector.detect_faces(self.dummy_image)
