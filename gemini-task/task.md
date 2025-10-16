You are an expert Python developer. Your task is to generate a complete **Python FastAPI service** for **face detection** using **MTCNN**.

Requirements:

1. **Service name:** FaceDetectionService
2. **Framework:** FastAPI
3. **Face detection library:** MTCNN (Python)
4. **Endpoints:**
   - `POST /detect`:
     - Accepts an image file upload (multipart/form-data)
     - Detects all faces in the image
     - Returns a JSON object with:
       - `faces`: array of detected faces
         - Each face includes:
           - `id` (unique temporary ID)
           - `bounding_box` (x, y, width, height)
           - `confidence` (detection confidence)
           - `thumbnail` (base64-encoded cropped face image)
   - Optional query param `return_crop` (boolean) to include cropped faces
5. **Features:**
   - Support images with **multiple faces**
   - Automatically generate **unique face IDs**
   - Crop faces and encode them in **base64** for frontend preview
   - Proper error handling (invalid image, no faces detected)
   - Logging of requests for debugging
6. **Folder structure suggestion:**
   - `app/` → main FastAPI app
   - `app/main.py` → entrypoint
   - `app/services/face_detector.py` → MTCNN detection logic
   - `app/models/` → Pydantic models for request/response
7. **Other requirements:**
   - Async endpoints
   - Use type hints everywhere
   - Include comments explaining each step
   - Ready to deploy (uvicorn or Docker)

**Output:** Generate a complete, ready-to-run Python FastAPI project implementing this face detection service. Do not include any UI code; only backend service code.

Remember: the service should be modular and maintainable, so that it can later be called from ASP.NET Core API for embedding generation and face labeling.
