import os
import dlib
import cv2
import numpy as np
from PIL import Image
from typing import List
import logging

import torch
from facenet_pytorch import InceptionResnetV1

# Configure logging
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter(
    '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
handler.setFormatter(formatter)
logger.addHandler(handler)


class FaceEmbeddingService:
    def __init__(self):
        try:
            # Load Dlib's face landmark predictor
            predictor_path = os.getenv(
                "DLIB_PREDICTOR_PATH",
                "app/models/shape_predictor_68_face_landmarks.dat"
            )
            self.predictor = dlib.shape_predictor(predictor_path)
            # Load Dlib's face recognition model
            encoder_path = os.getenv(
                "DLIB_ENCODER_PATH",
                "app/models/dlib_face_recognition_resnet_model_v1.dat"
            )
            self.face_encoder = dlib.face_recognition_model_v1(encoder_path)
            logger.info("FaceEmbeddingService initialized with Dlib models.")

            # Load FaceNet model
            self.device = torch.device(
                'cuda:0' if torch.cuda.is_available() else 'cpu'
            )
            self.resnet = (
                InceptionResnetV1(pretrained='vggface2')
                .eval()
                .to(self.device)
            )
            logger.info(f"FaceNet model initialized on {self.device}.")

        except Exception as e:
            logger.error(f"Error initializing Dlib face embedding models: {e}")
            raise

    def get_embedding(self, face_image: Image.Image) -> List[float]:
        """
        Generates a 128-dimensional face embedding for a given face image
        using Dlib.

        Args:
            face_image (Image.Image): A PIL Image object of the cropped face.

        Returns:
            List[float]: A list of 128 floats
            representing the face embedding.
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
            logger.info("Successfully generated Dlib face embedding.")
            return list(
                np.array(embedding).astype(np.float32)
            )
        except Exception as e:
            logger.error(f"Error generating Dlib face embedding: {e}")
            raise

    def get_facenet_embedding(self, face_image: Image.Image) -> List[float]:
        """
        Generates a 512-dimensional face embedding for a given face image
        using FaceNet (InceptionResnetV1).

        Args:
            face_image (Image.Image): A PIL Image object
                of the cropped face.

        Returns:
            List[float]: A list of 512 floats representing the face embedding.
        """
        try:
            # Preprocess image for FaceNet
            # FaceNet expects input in a specific format (e.g., 160x160, normalized)
            face_image_resized = face_image.resize(
                (160, 160)
            )
            img_np = np.array(face_image_resized).astype(np.float32)
            img_np = (img_np - 127.5) / 128.0  # Normalize to [-1, 1]
            img_tensor = torch.tensor(img_np).permute(2, 0, 1).unsqueeze(0).to(self.device)

            # Compute embedding
            self.resnet.eval()
            with torch.no_grad():
                embedding = (
                    self.resnet(img_tensor).squeeze().cpu().numpy()
                )

            logger.info("Successfully generated FaceNet embedding.")
            return list(embedding.astype(np.float32))  # noqa: E501
        except Exception as e:
            logger.error(f"Error generating FaceNet embedding: {e}")
            raise
