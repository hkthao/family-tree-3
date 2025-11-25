import face_recognition
import numpy as np
import cv2
import base64

def load_image_from_bytes(image_bytes: bytes):
    """Loads an image from bytes data."""
    try:
        nparr = np.frombuffer(image_bytes, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        return img
    except cv2.error: # Catch OpenCV decoding errors
        return None

def detect_faces(image_bytes: bytes):
    """
    Detects faces in an image (from bytes) and returns their bounding box locations.
    :param image_bytes: Image data in bytes.
    :return: List of face locations (top, right, bottom, left)
    :raises ValueError: If image data is invalid.
    """
    image = load_image_from_bytes(image_bytes)
    if image is None:
        raise ValueError("Could not load image from provided bytes.")
    
    try:
        # Convert the image from BGR color (which OpenCV uses) to RGB color (which face_recognition uses)
        rgb_image = image[:, :, ::-1]

        # Find all the face locations in the image
        face_locations = face_recognition.face_locations(rgb_image)
        return face_locations
    except Exception as e:
        # Catch exceptions from face_recognition itself
        print(f"Error in face detection: {e}")
        return []

def get_face_encodings(image_bytes: bytes):
    """
    Generates face encodings for detected faces in an image.
    :param image_bytes: Image data in bytes.
    :return: List of face encodings.
    """
    try:
        image = load_image_from_bytes(image_bytes)
        if image is None:
            raise ValueError("Could not load image from provided bytes.")
        
        rgb_image = image[:, :, ::-1]
        face_encodings = face_recognition.face_encodings(rgb_image)
        return face_encodings
    except Exception as e:
        print(f"Error in getting face encodings: {e}")
        return []

if __name__ == "__main__":
    # Example usage (assuming you have an image file named 'test_image.jpg')
    try:
        with open("test_image.jpg", "rb") as f:
            test_image_bytes = f.read()

        print("Detecting faces...")
        locations = detect_faces(test_image_bytes)
        print(f"Found {len(locations)} face(s) at: {locations}")

        if locations:
            print("Getting face encodings...")
            encodings = get_face_encodings(test_image_bytes)
            print(f"Found {len(encodings)} encoding(s). First encoding shape: {encodings[0].shape if encodings else 'N/A'}")

    except FileNotFoundError:
        print("Please create a 'test_image.jpg' file for example usage.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
