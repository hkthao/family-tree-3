import cv2
import numpy as np
import base64

def load_image_from_bytes(image_bytes: bytes):
    """Loads an image from bytes data."""
    nparr = np.frombuffer(image_bytes, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    return img

def encode_image_to_base64(image_np: np.ndarray):
    """Encodes a NumPy image array to base64 string."""
    _, buffer = cv2.imencode('.jpg', image_np)
    return base64.b64encode(buffer).decode('utf-8')

def crop_and_resize_face(image_bytes: bytes, face_location: tuple, target_size: int = 512):
    """
    Crops a face from an image based on face_location and resizes it to target_size x target_size.
    :param image_bytes: Original image data in bytes.
    :param face_location: Tuple (top, right, bottom, left) defining the face bounding box.
    :param target_size: The desired size for the cropped face (e.g., 512 for 512x512).
    :return: Base64 encoded string of the cropped and resized face, or None if error.
    """
    try:
        image = load_image_from_bytes(image_bytes)
        if image is None:
            raise ValueError("Could not load image from provided bytes.")

        top, right, bottom, left = face_location

        # Ensure bounding box coordinates are within image dimensions
        h, w, _ = image.shape
        top = max(0, top)
        right = min(w, right)
        bottom = min(h, bottom)
        left = max(0, left)

        # Crop the face
        cropped_face = image[top:bottom, left:right]

        # Check if cropped_face is empty (e.g., if bounding box was entirely out of image)
        if cropped_face.size == 0:
            print("Warning: Cropped face is empty, returning None.")
            return None

        # Resize the cropped face to target_size x target_size
        resized_face = cv2.resize(cropped_face, (target_size, target_size), interpolation=cv2.INTER_AREA)

        # Encode to base64
        return encode_image_to_base64(resized_face)

    except Exception as e:
        print(f"Error cropping and resizing face: {e}")
        return None

if __name__ == "__main__":
    # Example usage
    try:
        from detect_faces import detect_faces

        with open("test_image.jpg", "rb") as f:
            test_image_bytes = f.read()

        face_locations = detect_faces(test_image_bytes)

        if face_locations:
            print(f"Found {len(face_locations)} face(s). Cropping the first one.")
            first_face_location = face_locations[0]
            cropped_base64 = crop_and_resize_face(test_image_bytes, first_face_location)

            if cropped_base64:
                print("Face cropped, resized, and encoded to base64. Saving as 'cropped_face.jpg'")
                # Decode base64 and save to verify
                with open("cropped_face.jpg", "wb") as f:
                    f.write(base64.b64decode(cropped_base64))
            else:
                print("Failed to crop and resize face.")
        else:
            print("No faces detected to crop.")

    except FileNotFoundError:
        print("Please create a 'test_image.jpg' file for example usage.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
