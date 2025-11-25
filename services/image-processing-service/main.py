from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from fastapi.responses import JSONResponse, Response
from typing import List, Tuple, Optional
import base64
import json
import io
import cv2

from detect_faces import detect_faces, load_image_from_bytes
from crop_face import crop_and_resize_face, encode_image_to_base64
from get_emotion import get_emotion

app = FastAPI(
    title="Image Processing Service",
    description="Service for face detection, cropping, and emotion analysis.",
    version="1.0.0",
)

@app.get("/")
async def read_root():
    return {"message": "Image Processing Service is running!"}

@app.post("/detect-faces/")
async def detect_faces_endpoint(file: UploadFile = File(...)):
    """
    Detects faces in an uploaded image and returns their locations.
    """
    try:
        image_bytes = await file.read()
        face_locations = detect_faces(image_bytes)
        
        # Convert face locations to a more serializable format (list of dicts)
        serializable_face_locations = [
            {"top": top, "right": right, "bottom": bottom, "left": left}
            for (top, right, bottom, left) in face_locations
        ]
        
        return {"filename": file.filename, "face_locations": serializable_face_locations}
    except ValueError as e: # Catch ValueError specifically for invalid image data
        raise HTTPException(status_code=400, detail=f"Invalid image data: {e}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error detecting faces: {e}")

@app.post("/crop-and-analyze-face/")
async def crop_and_analyze_face_endpoint(
    file: UploadFile = File(...),
    face_location_json: str = Form(..., description="JSON string of face location (top, right, bottom, left)"),
    member_id: Optional[str] = Form(None, description="Optional member ID to associate with the face")
):
    """
    Crops a specific face from an uploaded image based on the provided location,
    resizes it to 512x512, and performs preliminary emotion analysis.
    Returns the base64 encoded cropped face and detected emotion.
    """
    try:
        image_bytes = await file.read()
        face_location_data = json.loads(face_location_json)
        
        # Convert dict to tuple
        face_location = (
            face_location_data["top"],
            face_location_data["right"],
            face_location_data["bottom"],
            face_location_data["left"],
        )
        
        # Crop and resize the face
        cropped_face_base64 = crop_and_resize_face(image_bytes, face_location, target_size=512)
        if not cropped_face_base64:
            raise HTTPException(status_code=400, detail="Failed to crop and resize face.")

        # Perform preliminary emotion analysis (uses placeholder for now)
        predicted_emotion, confidence = get_emotion(cropped_face_base64)

        return {
            "cropped_face_base64": cropped_face_base64,
            "emotion": predicted_emotion,
            "confidence": confidence,
            "member_id": member_id, # Return member_id if provided
        }
    except json.JSONDecodeError:
        raise HTTPException(status_code=400, detail="Invalid JSON format for face_location.")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing face: {e}")

@app.post("/normalize-image/")
async def normalize_image_endpoint(file: UploadFile = File(...)):
    """
    Normalizes an uploaded image to 512x512 and returns it as base64.
    """
    try:
        image_bytes = await file.read()
        image = load_image_from_bytes(image_bytes)
        if image is None:
            raise HTTPException(status_code=400, detail="Could not load image from provided bytes.")
        
        resized_image = cv2.resize(image, (512, 512), interpolation=cv2.INTER_AREA)
        normalized_image_base64 = encode_image_to_base64(resized_image)

        return {
            "normalized_image_base64": normalized_image_base64,
            "original_filename": file.filename,
        }
    except ValueError as e: # Catch ValueError specifically for image processing issues (e.g., from load_image_from_bytes)
        raise HTTPException(status_code=400, detail=f"Image processing error: {e}")
    except Exception as e: # Catch generic exceptions during processing
        print(f"DEBUG: Exception in normalize_image_endpoint: {e}") # Debug print
        raise HTTPException(status_code=500, detail=f"Error normalizing image: {e}")
