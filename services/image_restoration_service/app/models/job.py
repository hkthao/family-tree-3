from enum import Enum
from typing import List, Optional
from uuid import UUID

from pydantic import BaseModel, Field


class RestorationStatus(str, Enum):
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"


class RestorationJob(BaseModel):
    job_id: UUID = Field(..., description="Unique identifier for the restoration job.")
    status: RestorationStatus = Field(..., description="Current status of the restoration job.")
    original_url: str = Field(..., description="URL of the original image.")
    restored_url: Optional[str] = Field(None, description="URL of the restored image.")
    pipeline: List[str] = Field(default_factory=list, description="List of AI models used in the pipeline.")
    error: Optional[str] = Field(None, description="Error message if the job failed.")
