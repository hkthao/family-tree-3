import torch
from facenet_pytorch import InceptionResnetV1
from PIL import Image
import numpy as np
from typing import List

class FaceEmbeddingService:
    def __init__(self):
        self.device = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')
        self.resnet = InceptionResnetV1(pretrained='vggface2').eval().to(self.device)

    def get_embedding(self, face_image: Image.Image) -> List[float]:
        """
        Generates a 512-dimensional face embedding for a given face image.

        Args:
            face_image (Image.Image): A PIL Image object of the cropped face.

        Returns:
            List[float]: A list of 512 floats representing the face embedding.
        """
        # Preprocess the image for the FaceNet model
        # Resize to 160x160 and convert to tensor
        img_tensor = self.preprocess_image(face_image)

        # Generate embedding
        with torch.no_grad():
            embedding = self.resnet(img_tensor.unsqueeze(0).to(self.device))
        return embedding.squeeze().tolist()

    def preprocess_image(self, image: Image.Image) -> torch.Tensor:
        """
        Preprocesses a PIL Image for the FaceNet model.
        """
        image = image.resize((160, 160))
        image = np.transpose(np.array(image), (2, 0, 1))  # HWC to CHW
        image = (image - 127.5) / 128.0  # Normalize to [-1, 1]
        return torch.tensor(image, dtype=torch.float32)
