import dlib
import cv2
import numpy as np
from PIL import Image
from typing import List
import logging

# Configure logging
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)

class FaceEmbeddingService:
    def __init__(self):
        try:
            # Load Dlib's face landmark predictor
            self.predictor = dlib.shape_predictor("app/models/shape_predictor_68_face_landmarks.dat")
            # Load Dlib's face recognition model
            self.face_encoder = dlib.face_recognition_model_v1("app/models/dlib_face_recognition_resnet_model_v1.dat")
            logger.info("FaceEmbeddingService initialized with Dlib models.")
        except Exception as e:
            logger.error(f"Error initializing Dlib face embedding models: {e}")
            raise

    def get_embedding(self, face_image: Image.Image) -> List[float]:
        """
        Generates a 128-dimensional face embedding for a given face image using Dlib.

        Args:
            face_image (Image.Image): A PIL Image object of the cropped face.

        Returns:
            List[float]: A list of 128 floats representing the face embedding.
        """
        try:
            # Convert PIL Image to NumPy array (RGB)
            image_np = np.array(face_image.convert('RGB'))

            # Dlib's face detector works on grayscale images
            gray_image = cv2.cvtColor(image_np, cv2.COLOR_RGB2GRAY)

            # Create a dlib rectangle for the entire image, assuming it's a cropped face
            face_rect = dlib.rectangle(0, 0, image_np.shape[1], image_np.shape[0])

            # Detect facial landmarks
            landmarks = self.predictor(gray_image, face_rect)

            # Align the face using dlib.get_face_chip
            # This function takes the original image (image_np) and the landmarks
            aligned_face = dlib.get_face_chip(image_np, landmarks)

            # Compute the face descriptor (embedding) from the aligned face
            embedding = self.face_encoder.compute_face_descriptor(aligned_face, landmarks)
            logger.info("Successfully generated Dlib face embedding.")
            return list(embedding)
        except Exception as e:
            logger.error(f"Error generating Dlib face embedding: {e}")
            raise
