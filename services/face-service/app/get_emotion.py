import base64
import io
import numpy as np
from PIL import Image
import cv2
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.image import img_to_array

# Load the emotion detection model
emotion_model_path = 'app/models/emotion_model.hdf5'  # Update this path to your model
emotion_classifier = load_model(emotion_model_path, compile=False)
emotion_classifier.make_predict_function()
emotion_labels = [
    "Angry", "Disgust", "Fear", "Happy", "Neutral", "Sad", "Surprise"
]

def get_emotion(cropped_face_base64: str) -> tuple[str, float]:
    """
    Detects the emotion from a base64 encoded cropped face image.

    Args:
        cropped_face_base64: Base64 encoded string of the cropped face image.

    Returns:
        A tuple containing the predicted emotion (str) and its confidence (float).
    """
    try:
        # Decode base64 to image
        img_bytes = base64.b64decode(cropped_face_base64)
        image = Image.open(io.BytesIO(img_bytes)).convert('L')  # Convert to grayscale
        image = image.resize((48, 48))
        image = img_to_array(image)
        image = np.expand_dims(image, axis=0)
        image /= 255.0

        # Predict emotion
        emotion_prediction = emotion_classifier.predict(image)[0]
        emotion_probability = np.max(emotion_prediction)
        emotion_label_arg = np.argmax(emotion_prediction)
        predicted_emotion = emotion_labels[emotion_label_arg]

        return predicted_emotion, float(emotion_probability)
    except Exception as e:
        print(f"Error detecting emotion: {e}")
        return "Unknown", 0.0