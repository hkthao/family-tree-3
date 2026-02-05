from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
import logging
import subprocess

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

# Optional: Root endpoint for health check
@app.get("/")
async def root():
    return {"message": "Graphviz PDF Converter API is running."}
