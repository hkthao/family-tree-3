import numpy as np
import dlib
import cv2
import logging

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter(
    '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
handler.setFormatter(formatter)
logger.addHandler(handler)


class DlibFaceDetector:
    def __init__(self):
        self.detector = dlib.get_frontal_face_detector()
        logger.info("DlibFaceDetector initialized.")

    def detect_faces(self, image: np.ndarray):
        """
        Detects faces in a given image using Dlib's frontal face detector.

        Args:
            image (np.ndarray): The input image as a NumPy array (H, W, C).

        Returns:
            list: A list of detected faces, where each face is a dictionary
                  containing 'box' (bounding box) and 'confidence' (a dummy value for now).
        """
        try:
            # Convert image to grayscale for Dlib's detector
            gray_image = cv2.cvtColor(image, cv2.COLOR_RGB2GRAY)

            # Perform face detection
            detections = self.detector(gray_image, 1)  # Upsample the image 1 time

            detected_faces = []
            for d in detections:
                # Dlib returns dlib.rectangle objects, convert to dictionary
                detected_faces.append({
                    'box': [d.left(), d.top(), d.width(), d.height()],
                    'confidence': 0.99  # Dlib's detector doesn't provide a
                                        # confidence score directly, use a
                                        # high dummy value
                })
            logger.info(f"DlibFaceDetector detected {len(detected_faces)} faces.")
            return detected_faces
        except Exception as e:
            logger.error(f"Error in DlibFaceDetector.detect_faces: {e}")
            raise
