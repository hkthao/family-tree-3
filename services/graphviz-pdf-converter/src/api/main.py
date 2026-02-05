from fastapi import FastAPI, HTTPException, status, UploadFile, File, Form
from fastapi.middleware.cors import CORSMiddleware
from starlette.responses import FileResponse
import logging
import subprocess
import os
import uuid
from typing import Optional

from src.core.config import config
from src.core.domain import RenderRequest, RenderResponse
from src.core.services import GraphvizService

# Configure logging
logging.basicConfig(level=config.LOG_LEVEL, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Graphviz PDF Converter API",
    description="API for converting Graphviz DOT files to PDF.",
    version="1.0.0",
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # Allows all origins
    allow_credentials=True,
    allow_methods=["*"], # Allows all methods
    allow_headers=["*"], # Allows all headers
)

async def save_upload_file(upload_file: UploadFile, destination_dir: str) -> str:
    """Saves an uploaded file to the specified destination directory."""
    try:
        os.makedirs(destination_dir, exist_ok=True)
        unique_filename = f"{uuid.uuid4()}-{upload_file.filename}"
        file_path = os.path.join(destination_dir, unique_filename)
        contents = await upload_file.read()
        with open(file_path, "wb") as f:
            f.write(contents)
        logger.info("Saved uploaded file %s to %s", upload_file.filename, file_path)
        return unique_filename
    except Exception as e:
        logger.error("Failed to save uploaded file %s: %s", upload_file.filename, e)
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=f"Failed to save uploaded file: {e}")

@app.post("/render", response_model=RenderResponse, status_code=status.HTTP_200_OK)
async def render_graphviz_to_pdf(request: RenderRequest):
    """
    Renders a Graphviz DOT file to PDF.
    Expects a JSON body with job_id, dot_filename, page_size (optional), and direction (optional).
    """
    try:
        output_file_path = GraphvizService.render_dot_to_pdf(request)
        return RenderResponse(
            job_id=request.job_id,
            status="success",
            output_file_path=output_file_path
        )
    except FileNotFoundError as e:
        if "Input .dot file not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Input DOT file not found: {e}"
            )
        elif "Graphviz 'dot' command not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Graphviz 'dot' command not found on server: {e}"
            )
        else:
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Server error during rendering: {e}"
            )
    except subprocess.TimeoutExpired as e:
        raise HTTPException(
            status_code=status.HTTP_504_GATEWAY_TIMEOUT,
            detail=f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {e.stderr.decode().strip()}"
        )
    except RuntimeError as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Graphviz rendering failed: {e}"
        )
    except Exception as e:
        logger.exception("Unhandled error during PDF rendering for job %s: %s", request.job_id, e)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"An unexpected server error occurred: {e}"
        )

@app.post("/render-pdf-filename", response_model=RenderResponse, status_code=status.HTTP_200_OK)
async def render_pdf_filename(request: RenderRequest):
    """
    Renders a Graphviz DOT file to PDF and returns the path to the generated PDF file.
    Expects a JSON body with job_id, dot_filename, page_size (optional), and direction (optional).
    Assumes the .dot file specified by 'dot_filename' already exists in the configured INPUT_DIR.
    """
    try:
        output_file_path = GraphvizService.render_dot_to_pdf(request)
        return RenderResponse(
            job_id=request.job_id,
            status="success",
            output_file_path=output_file_path
        )
    except FileNotFoundError as e:
        if "Input .dot file not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Input DOT file not found: {e}"
            )
        elif "Graphviz 'dot' command not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Graphviz 'dot' command not found on server: {e}"
            )
        else:
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Server error during rendering: {e}"
            )
    except subprocess.TimeoutExpired as e:
        raise HTTPException(
            status_code=status.HTTP_504_GATEWAY_TIMEOUT,
            detail=f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {e.stderr.decode().strip()}"
        )
    except RuntimeError as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Graphviz rendering failed: {e}"
        )
    except Exception as e:
        logger.exception("Unhandled error during PDF rendering for job %s: %s", request.job_id, e)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"An unexpected server error occurred: {e}"
        )

@app.post("/render-and-download", status_code=status.HTTP_200_OK)
async def render_and_download_pdf(
    job_id: str = Form(...),
    dot_file: Optional[UploadFile] = File(None),
    dot_filename_path: Optional[str] = Form(None),
    page_size: str = Form("A0"),
    direction: str = Form("LR")
):
    """
    Renders a Graphviz DOT file (uploaded or from a path) to PDF and returns the PDF file.
    """
    input_dot_filename = None
    file_to_cleanup = None

    if dot_file:
        # Save the uploaded file to the input directory
        input_dot_filename = await save_upload_file(dot_file, config.INPUT_DIR)
        file_to_cleanup = os.path.join(config.INPUT_DIR, input_dot_filename)
    elif dot_filename_path:
        # Use the provided path as the dot_filename
        input_dot_filename = os.path.basename(dot_filename_path)
    else:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Either 'dot_file' (file upload) or 'dot_filename_path' (path to existing .dot file) must be provided."
        )

    try:
        render_request = RenderRequest(
            job_id=job_id,
            dot_filename=input_dot_filename,
            page_size=page_size,
            direction=direction
        )
        output_file_path = GraphvizService.render_dot_to_pdf(render_request)

        # Return the PDF file
        return FileResponse(
            path=output_file_path,
            media_type="application/pdf",
            filename=f"{job_id}.pdf"
        )
    except FileNotFoundError as e:
        if "Input .dot file not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Input DOT file not found: {e}"
            )
        elif "Graphviz 'dot' command not found" in str(e):
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Graphviz 'dot' command not found on server: {e}"
            )
        else:
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Server error during rendering: {e}"
            )
    except subprocess.TimeoutExpired as e:
        raise HTTPException(
            status_code=status.HTTP_504_GATEWAY_TIMEOUT,
            detail=f"Graphviz rendering timed out after {config.RENDER_TIMEOUT_SECONDS} seconds. Stderr: {e.stderr.decode().strip()}"
        )
    except RuntimeError as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Graphviz rendering failed: {e}"
        )
    except Exception as e:
        logger.exception("Unhandled error during PDF rendering for job %s: %s", job_id, e)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"An unexpected server error occurred: {e}"
        )
    finally:
        # Clean up the uploaded .dot file if it was temporary
        if file_to_cleanup and os.path.exists(file_to_cleanup):
            os.remove(file_to_cleanup)
            logger.info("Cleaned up temporary input file: %s", file_to_cleanup)


# Optional: Root endpoint for health check
@app.get("/")
async def root():
    return {"message": "Graphviz PDF Converter API is running."}
