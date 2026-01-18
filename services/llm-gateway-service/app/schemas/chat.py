from typing import List, Dict, Any, Optional
from pydantic import BaseModel, Field

# Request Models
class ChatMessage(BaseModel):
    role: str
    content: str

class ChatCompletionRequest(BaseModel):
    model: str
    messages: List[ChatMessage]
    temperature: float = Field(default=0.0, ge=0.0, le=2.0)
    max_tokens: Optional[int] = Field(default=512, ge=1)
    stream: bool = False

# Response Models (Simplified for our gateway)
class ChatCompletionMessage(BaseModel):
    role: str
    content: str

class ChatCompletionChoice(BaseModel):
    index: int = 0
    message: ChatCompletionMessage
    finish_reason: str = "stop"

class ChatCompletionResponse(BaseModel):
    id: str = "chatcmpl-001" # Static for simplicity as per requirement
    object: str = "chat.completion"
    choices: List[ChatCompletionChoice]
