from pydantic import BaseModel
from typing import Optional

class RenderRequest(BaseModel):
    job_id: str
    dot_filename: str
    page_size: str = "A0"
    direction: str = "LR"

class RenderResponse(BaseModel):
    job_id: str
    status: str
    output_file_path: Optional[str] = None
    error_message: Optional[str] = None
