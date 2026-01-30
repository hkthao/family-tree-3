from abc import ABC, abstractmethod
from typing import List, Dict, Any
import numpy as np

class IFaceDetector(ABC):
    """
    Abstract Base Class for face detection services.
    Defines the contract for any class that provides face detection functionality.
    """
    @abstractmethod
    def detect_faces(self, image: np.ndarray) -> List[Dict[str, Any]]:
        """
        Detects faces in a given image.

        Args:
            image (np.ndarray): The input image as a NumPy array (e.g., from PIL.Image.open and np.array).

        Returns:
            List[Dict[str, Any]]: A list of dictionaries, where each dictionary
                                  represents a detected face and contains its bounding box
                                  (x, y, w, h) and confidence score.
                                  Example: [{'box': [x, y, w, h], 'confidence': score}]
        """
        pass
