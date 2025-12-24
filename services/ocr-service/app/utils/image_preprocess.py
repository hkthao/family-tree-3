from PIL import Image
import numpy as np
import cv2

def preprocess_image_for_ocr(image: Image.Image) -> Image.Image:
    """
    Applies basic preprocessing steps to an image to improve OCR quality.
    - Converts to grayscale.
    - Applies Otsu's thresholding.
    """
    # Convert PIL Image to OpenCV format
    img_np = np.array(image.convert('L')) # Convert to grayscale directly

    # Apply Otsu's thresholding
    _, thresh = cv2.threshold(
        img_np, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU
    )

    # Convert back to PIL Image
    return Image.fromarray(thresh)
