# Image Restoration Service

This service provides an API for restoring old images using AI models hosted on Replicate. It utilizes a two-step pipeline: Face Restoration (GFPGAN) and Image Upscaling (Real-ESRGAN).

## Features

-   **Face Restoration**: Uses `tencentarc/gfpgan` to restore blurred, cracked, or noisy faces.
-   **Image Upscaling**: Uses `nightmareai/real-esrgan` to enhance the resolution of the restored image.
-   **Asynchronous Processing**: AI restoration jobs run in the background.
-   **API Endpoints**:
    -   `POST /restore`: Start a new restoration job.
    -   `GET /restore/{jobId}`: Check the status and retrieve results of a job.

## Technology Stack

-   **Language**: Python 3.10+
-   **AI Platform**: Replicate
-   **API Framework**: FastAPI
-   **Dependency Management**: pip

## Project Structure

```
image_restoration_service/
│
├── app/
│   ├── main.py              # FastAPI entrypoint
│   ├── api.py               # API routes
│   ├── services/
│   │   └── replicate_service.py # Logic for interacting with Replicate API
│   ├── models/
│   │   └── job.py            # Pydantic models for job state
│   └── config.py            # Environment configuration
│
├── requirements.txt         # Python dependencies
└── README.md                # Project README
```

## Setup and Running Locally

### Prerequisites

-   Python 3.10+
-   `pip` (Python package installer)
-   `REPLICATE_API_TOKEN`: Obtain your API token from [Replicate](https://replicate.com/account).

### 1. Clone the repository (if not already done)

```bash
# Assuming you are in the root of your family-tree-3 project
cd services/image_restoration_service
```

### 2. Install dependencies

```bash
pip install -r requirements.txt
```

### 3. Set up environment variables

Create a `.env` file in the `image_restoration_service/` directory and add your Replicate API token:

```
REPLICATE_API_TOKEN="r8_YOUR_REPLICATE_API_TOKEN_HERE"
```

**IMPORTANT**: Replace `"r8_YOUR_REPLICATE_API_TOKEN_HERE"` with your actual Replicate API token.

### 4. Run the service

Navigate to the `image_restoration_service/app` directory and run the FastAPI application using Uvicorn:

```bash
cd app
uvicorn main:app --reload
```

The service will be running at `http://127.0.0.1:8000`.

### 5. Access the API Documentation

Once the service is running, you can access the interactive API documentation (Swagger UI) at:

[http://127.0.0.1:8000/docs](http://127.00.0.1:8000/docs)

You can use this interface to test the `/restore` and `/restore/{jobId}` endpoints.

### Running with Docker

1.  **Build the Docker image:**

    ```bash
    docker build -t image-restoration-service .
    ```

2.  **Run the Docker container:**

    ```bash
    docker run -d -p 8000:8000 --name image-restoration-app -e REPLICATE_API_TOKEN="r8_YOUR_REPLICATE_API_TOKEN_HERE" image-restoration-service
    ```

    **IMPORTANT**: Replace `"r8_YOUR_REPLICATE_API_TOKEN_HERE"` with your actual Replicate API token.

    The service will be accessible at `http://localhost:8000`.

## API Endpoints

### 1. Start Image Restoration Job

`POST /restore`

Starts a new asynchronous image restoration job.

**Request Body Example:**

```json
{
  "imageUrl": "https://upload.wikimedia.org/wikipedia/commons/4/47/George_Washington_-_Naval_Portrait_at_34.jpg"
}
```

**Response Example (immediately returns job status):**

```json
{
  "job_id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "status": "processing",
  "original_url": "https://upload.wikimedia.org/wikipedia/commons/4/47/George_Washington_-_Naval_Portrait_at_34.jpg",
  "restored_url": null,
  "pipeline": [],
  "error": null
}
```

### 2. Check Job Status

`GET /restore/{jobId}`

Retrieves the current status and results of a specified restoration job.

**Response Example (if still processing):**

```json
{
  "job_id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "status": "processing",
  "original_url": "https://upload.wikimedia.org/wikipedia/commons/4/47/George_Washington_-_Naval_Portrait_at_34.jpg",
  "restored_url": null,
  "pipeline": [],
  "error": null
}
```

**Response Example (if completed):**

```json
{
  "job_id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "status": "completed",
  "original_url": "https://upload.wikimedia.org/wikipedia/commons/4/47/George_Washington_-_Naval_Portrait_at_34.jpg",
  "restored_url": "https://replicate.delivery/pbxt/...",
  "pipeline": ["GFPGAN", "Real-ESRGAN"],
  "error": null
}
```

**Response Example (if failed):**

```json
{
  "job_id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "status": "failed",
  "original_url": "https://upload.wikimedia.org/wikipedia/commons/4/47/George_Washington_-_Naval_Portrait_at_34.jpg",
  "restored_url": null,
  "pipeline": ["GFPGAN"],
  "error": "Real-ESRGAN failed: An error occurred during Replicate prediction: ..."
}
```
