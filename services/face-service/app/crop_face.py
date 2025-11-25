import io
import base64
from PIL import Image

def crop_and_resize_face(image_bytes: bytes, face_location: tuple, target_size: int = 512) -> str:
    """
    Crops a face from an image based on the provided location and resizes it.

    Args:
        image_bytes: The image as bytes.
        face_location: A tuple (top, right, bottom, left) defining the face's bounding box.
        target_size: The desired size (width and height) for the resized image.

    Returns:
        A base64 encoded string of the cropped and resized face image, or None if cropping fails.
    """
    try:
        image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        top, right, bottom, left = face_location
        cropped_face = image.crop((left, top, right, bottom))
        resized_face = cropped_face.resize((target_size, target_size), Image.LANCZOS)
        
        buffered = io.BytesIO()
        resized_face.save(buffered, format="PNG")
        return base64.b64encode(buffered.getvalue()).decode("utf-8")
    except Exception as e:
        print(f"Error cropping and resizing face: {e}")
        return None

def encode_image_to_base64(image: Image.Image, format: str = "PNG") -> str:
    """
    Encodes a PIL Image object to a base64 string.
    """
    buffered = io.BytesIO()
    image.save(buffered, format=format)
    return base64.b64encode(buffered.getvalue()).decode("utf-8")

def resize_image(image_bytes: bytes, target_size: int = 512) -> str:
    """
    Resizes an image given its bytes to a target_size x target_size.
    """
    try:
        image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        resized_image = image.resize((target_size, target_size), Image.LANCZOS)
        return encode_image_to_base64(resized_image)
    except Exception as e:
        print(f"Error resizing image: {e}")
        return None