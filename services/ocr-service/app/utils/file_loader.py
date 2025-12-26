import io
from typing import List
from PIL import Image
from pdf2image import convert_from_bytes

def load_image_from_bytes(image_bytes: bytes) -> Image.Image:
    """
    Loads an image from bytes.
    """
    return Image.open(io.BytesIO(image_bytes)).convert("RGB")

def load_pdf_as_images(pdf_bytes: bytes) -> List[Image.Image]:
    """
    Loads a PDF from bytes and converts each page into a PIL Image.
    """
    # Use 300 DPI for better OCR accuracy.
    # poppler_path might be needed if poppler is not in PATH
    return convert_from_bytes(pdf_bytes, dpi=300)
