from abc import ABC, abstractmethod
from typing import List
from PIL.Image import Image as PILImage

class IFaceEmbedding(ABC):
    """
    Abstract Base Class for face embedding services.
    Defines the contract for any class that provides face embedding functionality.
    """
    @abstractmethod
    def get_embedding(self, face_image: PILImage) -> List[float]:
        """
        Generates a 128-dimensional face embedding for a given cropped face image.

        Args:
            face_image (PILImage): A cropped PIL Image containing a single face.

        Returns:
            List[float]: A list of floats representing the 128-dimensional face embedding.
        """
        pass
