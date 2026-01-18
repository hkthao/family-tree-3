from fastapi import APIRouter, HTTPException, status

from ..core.lancedb import lancedb_service
from ..schemas.vectors import (
    AddVectorRequest, UpdateVectorRequest, DeleteVectorRequest,
    RebuildVectorRequest
)


router = APIRouter()


@router.post("/add", status_code=status.HTTP_201_CREATED)
async def add_vectors(request: AddVectorRequest):
    """
    Adds new vector entries to the LanceDB table.
    """
    try:
        # Assuming all vectors in a request belong to the same family
        # for table naming
        if not request.vectors:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST,
                                detail="No vectors provided.")

        family_id = request.vectors[0].family_id
        table_name = lancedb_service._get_table_name(family_id)

        # Ensure table exists before adding.
        # create_dummy_table also handles recreation.
        lancedb_service.create_dummy_table(family_id)

        lancedb_service.add_vectors(table_name, request.vectors)
        return {"message": (f"Successfully added {len(request.vectors)} "
                            f"vectors to family {family_id}.")}
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                            detail=str(e))


@router.put("/update", status_code=status.HTTP_200_OK)
async def update_vectors(request: UpdateVectorRequest):
    """
    Updates existing vector entries in the LanceDB table.
    """
    try:
        table_name = lancedb_service._get_table_name(request.family_id)
        if table_name not in lancedb_service.db.table_names():
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Family table '{table_name}' not found."
            )

        lancedb_service.update_vectors(table_name, request)
        return {"message": (f"Successfully updated vectors for family "
                            f"{request.family_id}.")}
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                            detail=str(e))


@router.delete("/delete", status_code=status.HTTP_200_OK)
async def delete_vectors(request: DeleteVectorRequest):
    """
    Deletes vector entries from the LanceDB table.
    """
    try:
        table_name = lancedb_service._get_table_name(request.family_id)
        if table_name not in lancedb_service.db.table_names():
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Family table '{table_name}' not found."
            )

        lancedb_service.delete_vectors(table_name, request)
        return {"message": (f"Successfully deleted vectors for family "
                            f"{request.family_id}.")}
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                            detail=str(e))


@router.post("/rebuild", status_code=status.HTTP_200_OK)
async def rebuild_vectors(request: RebuildVectorRequest):
    """
    Triggers a rebuild process for vectors in the LanceDB table.
    This is a simulation and in a real scenario would involve re-embedding data.
    """
    try:
        lancedb_service.rebuild_vectors(request)
        return {"message": (f"Rebuild process triggered for family "
                            f"{request.family_id}.")}
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                            detail=str(e))
