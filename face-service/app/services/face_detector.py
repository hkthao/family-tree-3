import numpy as np
from mtcnn import MTCNN

class MTCNNFaceDetector:
    def __init__(self):
        # Initialize MTCNN detector
        self.detector = MTCNN()

    def detect_faces(self, image: np.ndarray):
        """
        Detects faces in a given image using MTCNN.

        Args:
            image (np.ndarray): The input image as a NumPy array (H, W, C).

        Returns:
            list: A list of detected faces, where each face is a dictionary
                  containing 'box' (bounding box) and 'confidence'.
        """
        # MTCNN expects RGB image, ensure the input is in the correct format
        # If the image is already RGB, this conversion does nothing.
        # If it's BGR (e.g., from OpenCV), it converts it to RGB.
        if image.ndim == 3 and image.shape[2] == 3:
            # Assuming image is already RGB or needs no conversion if it's a standard image
            pass
        elif image.ndim == 2:
            # Convert grayscale to RGB if necessary
            image = np.stack([image, image, image], axis=-1)
        else:
            raise ValueError("Unsupported image format. Image must be 2D (grayscale) or 3D (RGB).")

        # Perform face detection
        detections = self.detector.detect_faces(image)
        return detections
