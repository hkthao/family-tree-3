from typing import Any, Optional, TypeVar, Generic
from pydantic import BaseModel
from datetime import datetime

# Generics for KnowledgeIndexRequest
T = TypeVar("T")

class FamilyKnowledgeDto(BaseModel):
    family_id: str # Changed to str to match Guid.ToString() from C#
    name: str
    code: str
    description: Optional[str] = None
    genealogy_record: Optional[str] = None
    progenitor_name: Optional[str] = None
    family_covenant: Optional[str] = None
    visibility: str = "Private"
    content_type: str = "Family"

class MemberKnowledgeDto(BaseModel):
    family_id: str # Changed to str
    member_id: str # Changed to str
    full_name: str
    code: str
    gender: Optional[str] = None
    biography: Optional[str] = None
    date_of_birth: Optional[datetime] = None
    date_of_death: Optional[datetime] = None
    content_type: str = "Member"

class EventKnowledgeDto(BaseModel):
    family_id: str # Changed to str
    event_id: str # Changed to str
    name: str
    code: str
    description: Optional[str] = None
    location: Optional[str] = None
    calendar_type: Optional[str] = None
    solar_date: Optional[datetime] = None
    content_type: str = "Event"

class KnowledgeIndexRequest(BaseModel, Generic[T]):
    data: T
    action: str = "index" # "index" or "delete"

