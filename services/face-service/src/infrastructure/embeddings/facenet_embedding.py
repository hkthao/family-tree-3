import dlib
import cv2
import numpy as np
from PIL import Image
from typing import List
import logging

from src.domain.interfaces.face_embedding import IFaceEmbedding


# Configure logging
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter(
    '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
handler.setFormatter(formatter)
logger.addHandler(handler)


class FaceNetEmbeddingService(IFaceEmbedding):
    def __init__(self):
        try:
            # Load Dlib's face landmark predictor
            predictor_path = "app/models/dlib_models/shape_predictor_68_face_landmarks.dat"
            self.predictor = dlib.shape_predictor(predictor_path)
            # Load Dlib's face recognition model
            encoder_path = "app/models/dlib_models/dlib_face_recognition_resnet_model_v1.dat"
            self.face_encoder = dlib.face_recognition_model_v1(encoder_path)
            logger.info("FaceNetEmbeddingService initialized with Dlib models.")

        except Exception as e:
            logger.error(f"Error initializing Dlib face embedding models: {e}")
            raise

    def get_embedding(self, face_image: Image.Image) -> List[float]:
        """
        Generates a 128-dimensional face embedding (Non-normalized) for a given face image
        using Dlib.

        Args:
            face_image (Image.Image): A PIL Image object of the cropped face.

        Returns:
            List[float]: A list of 128 floats
            representing the face
            embedding.
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
            embedding = self.face_encoder.compute_face_descriptor(
                aligned_face, landmarks
            )

            # Convert to numpy array for normalization
            embedding_np = np.array(embedding).astype(np.float32)
            # Perform L2 normalization
            norm = np.linalg.norm(embedding_np)
            if norm > 0:
                normalized_embedding = embedding_np / norm
            else:
                normalized_embedding = embedding_np # Avoid division by zero, though unlikely for face embeddings

            logger.info("Successfully generated and L2-normalized Dlib face embedding.")
            return [float(x) for x in normalized_embedding]
        except Exception as e:
            logger.error(f"Error generating Dlib face embedding: {e}")
            raise
