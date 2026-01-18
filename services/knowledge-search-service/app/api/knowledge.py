from fastapi import APIRouter, Depends, HTTPException, status
from typing import Union
from loguru import logger

from ..core.lancedb import lancedb_service
from ..core.embeddings import embedding_service
from ..schemas.vectors import VectorData, DeleteVectorRequest
from ..schemas.knowledge_dtos import (
    FamilyKnowledgeDto,
    MemberKnowledgeDto,
    EventKnowledgeDto,
    KnowledgeIndexRequest,
)

router = APIRouter()

@router.post("/family-data", status_code=status.HTTP_200_OK)
async def process_family_knowledge(
    request: KnowledgeIndexRequest[FamilyKnowledgeDto],
):
    logger.info(
        "Received family knowledge request for family_id: {family_id}, action: {action}",
        request.data.family_id,
        request.action,
    )
    return await _process_knowledge_request(
        request,
        lambda dto: f"{dto.name} {dto.description} {dto.genealogy_record} {dto.progenitor_name} {dto.family_covenant}",
        "family_id",
        request.data.family_id,
        request.data.family_id,
        request.data.content_type,
    )


@router.post("/member-data", status_code=status.HTTP_200_OK)
async def process_member_knowledge(
    request: KnowledgeIndexRequest[MemberKnowledgeDto],
):
    logger.info(
        "Received member knowledge request for member_id: {member_id}, action: {action}",
        request.data.member_id,
        request.action,
    )
    return await _process_knowledge_request(
        request,
        lambda dto: f"{dto.full_name} {dto.biography} {dto.gender}",
        "member_id",
        request.data.member_id,
        request.data.family_id,
        request.data.content_type,
    )


@router.post("/event-data", status_code=status.HTTP_200_OK)
async def process_event_knowledge(
    request: KnowledgeIndexRequest[EventKnowledgeDto],
):
    logger.info(
        "Received event knowledge request for event_id: {event_id}, action: {action}",
        request.data.event_id,
        request.action,
    )
    return await _process_knowledge_request(
        request,
        lambda dto: f"{dto.name} {dto.description} {dto.location} {dto.calendar_type} {dto.solar_date}",
        "event_id",
        request.data.event_id,
        request.data.family_id,
        request.data.content_type,
    )


async def _process_knowledge_request(
    request: Union[
        KnowledgeIndexRequest[FamilyKnowledgeDto],
        KnowledgeIndexRequest[MemberKnowledgeDto],
        KnowledgeIndexRequest[EventKnowledgeDto],
    ],
    text_extractor: callable,
    id_field_name: str,
    id_value: str,
    family_id: str,
    content_type: str,
):
    table_name = lancedb_service._get_table_name(family_id)

    if request.action == "index":
        content_to_embed = text_extractor(request.data)
        embedding = embedding_service.get_embedding(content_to_embed)

        metadata = request.data.model_dump()
        metadata["text_content"] = content_to_embed # Add for potential future use or debugging

        vector_data = VectorData(
            id=id_value,
            vector=embedding,
            family_id=family_id,
            content_type=content_type,
            metadata=metadata,
        )

        lancedb_service.add_vectors(table_name, [vector_data])
        return {"message": f"{content_type} data for ID {id_value} indexed successfully."}

    elif request.action == "delete":
        delete_request = DeleteVectorRequest(
            family_id=family_id,
            # LanceDB delete expects a list of IDs to delete
            # We can leverage the filter capability if needed for more complex deletes
            where_clause=f"{id_field_name} = '{id_value}'"
        )
        lancedb_service.delete_vectors(table_name, delete_request)
        return {"message": f"{content_type} data for ID {id_value} deleted successfully."}
    else:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST, detail="Invalid action specified."
        )

