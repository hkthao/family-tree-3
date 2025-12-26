import pytesseract
from PIL import Image
from typing import List
import logging

from app.utils.image_preprocess import preprocess_image_for_ocr
from app.utils.text_cleaner import clean_ocr_text

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

class OCRService:
    def __init__(self):
        # Ensure Tesseract is in PATH or specify the path to tesseract executable
        # pytesseract.pytesseract.tesseract_cmd = '/usr/bin/tesseract' # Example
        logger.info("OCRService initialized.")

    def ocr_image(self, image: Image.Image, lang: str = 'eng+vie') -> str:
        """
        Performs OCR on a single PIL Image, with optional preprocessing.
        Supports English and Vietnamese.
        """
        try:
            processed_image = preprocess_image_for_ocr(image)
            text = pytesseract.image_to_string(processed_image, lang=lang)
            cleaned_text = clean_ocr_text(text)
            logger.info(f"OCR performed on image. Extracted text length: {len(cleaned_text)}")
            return cleaned_text
        except Exception as e:
            logger.error(f"Error during OCR on image: {e}")
            raise

    def ocr_images(self, images: List[Image.Image], lang: str = 'eng+vie') -> str:
        """
        Performs OCR on a list of PIL Images (e.g., from a PDF),
        concatenating the results.
        """
        full_text = []
        for i, image in enumerate(images):
            logger.info(f"Performing OCR on image {i+1}/{len(images)}")
            try:
                page_text = self.ocr_image(image, lang=lang)
                full_text.append(page_text)
            except Exception as e:
                logger.warning(f"OCR failed for image {i+1}: {e}")
                # Continue with other pages even if one fails
                full_text.append("")

        combined_text = "\n".join(full_text)
        cleaned_combined_text = clean_ocr_text(combined_text)
        logger.info(f"OCR completed for {len(images)} images. Total extracted text length: {len(cleaned_combined_text)}")
        return cleaned_combined_text
