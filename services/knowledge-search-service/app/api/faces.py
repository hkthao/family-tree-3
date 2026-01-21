from fastapi import APIRouter, HTTPException, status, Depends, Request
from uuid import UUID
from loguru import logger  # Add this import
from ..core.lancedb import (
    FaceLanceDBService,
    FACE_LANCEDB_SCHEMA,
)  # Import the class and SCHEMA
from ..core.face_embeddings import (
    FaceEmbeddingService,
    face_embedding_service,
)  # Import the face embedding service
from ..models.face_models import (
    FaceEmbeddingResponse,
    AddFaceRequest,
    FaceMetadata,
    UpdateFaceRequest,
    SearchFaceRequest,
    SearchFacesResponse,
    SearchResult,
)

router = APIRouter()


# Dependencies
def get_face_lancedb_service(request: Request) -> FaceLanceDBService:
    return request.app.state.face_lancedb_service


def get_face_embedding_service() -> FaceEmbeddingService:
    return face_embedding_service


@router.get("/faces/test", status_code=status.HTTP_200_OK)
async def test_faces_endpoint():
    return {"message": "Faces test endpoint reachable."}


@router.post(
    "/faces",
    response_model=FaceEmbeddingResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Thêm thông tin khuôn mặt (bao gồm vector nhúng) vào cơ sở dữ liệu LanceDB.",
)
async def add_face(
    request: AddFaceRequest,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
    face_embed_service: FaceEmbeddingService = Depends(get_face_embedding_service),
):
    """
    Thêm thông tin chi tiết về một khuôn mặt, bao gồm cả vector nhúng của nó,
    vào cơ sở dữ liệu LanceDB. Nếu vector nhúng không được cung cấp, nó sẽ
    được tạo ra từ original_image_url sử dụng dịch vụ nhúng.
    """
    try:
        # Explicitly ensure the table exists and has the correct schema
        # This will drop and recreate if dimensions mismatch
        await lancedb_service._create_table_if_not_exists(
            lancedb_service._get_face_table_name(str(request.family_id)),
            FACE_LANCEDB_SCHEMA,
        )
        logger.info(
            f"Incoming add_face request: {request.model_dump_json()}"
        )  # Log incoming request

        # request is now directly the FaceMetadata
        if not request.embedding:
            if not request.original_image_url:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Cần cung cấp 'embedding' hoặc 'original_image_url' để tạo nhúng.",
                )
            # Use the face embedding service to create embedding from image URL
            request.embedding = face_embed_service.embed_image_url(
                request.original_image_url
            )
            logger.info(
                f"Generated embedding from original_image_url for face_id: "
                f"{request.face_id}"
            )

        # Tạo FaceId nếu chưa có
        if not request.face_id:
            request.face_id = UUID(str(UUID.random_uuid()))

        # Lưu thông tin khuôn mặt vào LanceDB
        face_data_dict = request.model_dump()

        logger.debug(
            f"Attempting to add face data to LanceDB for family_id: {request.family_id}, face_id: {request.face_id}"
        )
        await lancedb_service.add_face_data(str(request.family_id), [face_data_dict])
        logger.debug("Successfully added face data to LanceDB.")

        return FaceEmbeddingResponse(
            face_id=request.face_id,
            member_id=request.member_id,
            vector_db_id=request.vector_db_id,
            message="Thông tin khuôn mặt đã được thêm thành công.",
        )
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.exception(
            f"Đã xảy ra lỗi nội bộ khi thêm khuôn mặt: {e}"
        )  # Log full traceback
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi thêm khuôn mặt: {e}",
        )


@router.post(
    "/faces/search",
    response_model=SearchFacesResponse,
    summary="Tìm kiếm các khuôn mặt tương tự dựa trên vector nhúng đầu vào hoặc URL hình ảnh.",
)
async def search_faces(
    request: SearchFaceRequest,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
    face_embed_service: FaceEmbeddingService = Depends(get_face_embedding_service),
):
    # logger.info(f"Incoming search_faces request: {request.model_dump_json()}") # Add this line
    try:
        query_embedding = request.query_embedding
        if not query_embedding:
            if not request.image_url:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Cần cung cấp 'query_embedding' hoặc 'image_url' để tìm kiếm.",
                )
            # Use the face embedding service to create embedding from image URL for search
            query_embedding = face_embed_service.embed_image_url(request.image_url)
            logger.info("Generated query embedding from image_url for search.")
        # Thực hiện tìm kiếm vector trong LanceDB
        results = await lancedb_service.search_faces(
            family_id=str(request.family_id),
            query_embedding=query_embedding,
            member_id=str(request.member_id)
            if request.member_id
            else None,  # Optional filter
            top_k=request.top_k,
        )
        # Chuyển đổi kết quả từ LanceDB sang định dạng SearchResult
        search_results = []
        for res in results:
            # Tạo một đối tượng FaceMetadata từ các trường đã deserialize
            face_metadata_obj = FaceMetadata(
                family_id=res.get("family_id"),
                member_id=res.get("member_id"),
                face_id=res.get("face_id"),
                bounding_box=res.get("bounding_box"),
                confidence=res.get("confidence"),
                thumbnail_url=res.get("thumbnail_url"),
                original_image_url=res.get("original_image_url"),
                emotion=res.get("emotion"),
                emotion_confidence=res.get("emotion_confidence"),
                vector_db_id=res.get("vector_db_id"),
                is_vector_db_synced=res.get("is_vector_db_synced"),
            )
            search_results.append(
                SearchResult(
                    face_id=res.get("face_id"),
                    member_id=res.get("member_id"),
                    score=res.get("score"),
                    vector_db_id=res.get("vector_db_id"),  # Pass vector_db_id here
                    metadata=face_metadata_obj,
                )
            )
        return SearchFacesResponse(results=search_results)
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.exception(
            f"Đã xảy ra lỗi nội bộ khi tìm kiếm khuôn mặt: {e}"
        )  # Log full traceback
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi tìm kiếm khuôn mặt: {e}",
        )


@router.delete(
    "/faces/{family_id}/{face_id}",
    status_code=status.HTTP_200_OK,
    summary="Xóa một khuôn mặt cụ thể khỏi cơ sở dữ liệu LanceDB.",
)
async def delete_face(
    family_id: UUID,
    face_id: UUID,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
):
    """
    Xóa một khuôn mặt cụ thể dựa trên face_id của nó trong một family_id nhất định.
    """
    try:
        deleted_count = await lancedb_service.delete_face_data(
            family_id=str(family_id), face_id=str(face_id)
        )
        if deleted_count == 0:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Không tìm thấy khuôn mặt với face_id '{face_id}' trong family_id '{family_id}' để xóa.",
            )
        return {"message": f"Khuôn mặt với face_id '{face_id}' đã được xóa thành công."}
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi xóa khuôn mặt: {e}",
        )


@router.delete(
    "/faces/member/{family_id}/{member_id}",
    status_code=status.HTTP_200_OK,
    summary="Xóa tất cả khuôn mặt liên quan đến một thành viên khỏi cơ sở dữ liệu LanceDB.",
)
async def delete_faces_by_member_id(
    family_id: UUID,
    member_id: UUID,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
):
    """
    Xóa tất cả các khuôn mặt liên quan đến một member_id cụ thể trong một family_id nhất định.
    """
    try:
        deleted_count = await lancedb_service.delete_face_data(
            family_id=str(family_id), member_id=str(member_id)
        )
        if deleted_count == 0:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Không tìm thấy khuôn mặt nào cho member_id '{member_id}' trong family_id '{family_id}' để xóa.",
            )
        return {
            "message": f"Tất cả khuôn mặt của thành viên '{member_id}' trong family_id '{family_id}' đã được xóa thành công."
        }
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi xóa khuôn mặt của thành viên: {e}",
        )


@router.delete(
    "/faces/family/{family_id}",
    status_code=status.HTTP_200_OK,
    summary="Xóa tất cả khuôn mặt của một gia đình khỏi cơ sở dữ liệu LanceDB.",
)
async def delete_faces_by_family_id(
    family_id: UUID,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
):
    """
    Xóa tất cả các khuôn mặt liên quan đến một family_id cụ thể.
    """
    try:
        await lancedb_service.delete_faces_by_family_id(family_id=str(family_id))
        return {
            "message": f"Tất cả khuôn mặt trong family_id '{family_id}' đã được xóa thành công."
        }
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.exception(f"Đã xảy ra lỗi nội bộ khi xóa khuôn mặt theo gia đình: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi xóa khuôn mặt theo gia đình: {e}",
        )


@router.put(
    "/faces/{family_id}/{face_id}",
    response_model=FaceEmbeddingResponse,
    summary="Cập nhật thông tin của một khuôn mặt hiện có trong cơ sở dữ liệu LanceDB.",
)
async def update_face(
    family_id: UUID,
    face_id: UUID,
    request: UpdateFaceRequest,
    lancedb_service: FaceLanceDBService = Depends(get_face_lancedb_service),
    face_embed_service: FaceEmbeddingService = Depends(get_face_embedding_service),
):
    """
    Cập nhật thông tin chi tiết về một khuôn mặt hiện có trong cơ sở dữ liệu LanceDB.
    """
    try:
        # Chuyển đổi UpdateFaceRequest thành dict để cập nhật
        # exclude_unset=True chỉ bao gồm các trường được set trong request
        update_data = request.model_dump(exclude_unset=True)
        # Loại bỏ face_id khỏi dữ liệu cập nhật để tránh ghi đè sai
        update_data.pop("face_id", None)
        # If original_image_url is updated and embedding is not provided, generate new embedding
        if "original_image_url" in update_data and "embedding" not in update_data:
            update_data["embedding"] = face_embed_service.embed_image_url(
                update_data["original_image_url"]
            )
            logger.info(
                f"Generated new embedding from original_image_url for face_id: {face_id}"
            )
        updated_count = await lancedb_service.update_face_data(
            family_id=str(family_id), face_id=str(face_id), update_data=update_data
        )
        if updated_count == 0:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Không tìm thấy khuôn mặt với face_id '{face_id}' trong family_id '{family_id}' để cập nhật.",
            )
        response_member_id = request.member_id if request.member_id else None
        response_vector_db_id = update_data.get("vector_db_id")
        return FaceEmbeddingResponse(
            face_id=face_id,
            member_id=response_member_id,
            vector_db_id=response_vector_db_id,
            message=f"Thông tin khuôn mặt với face_id '{face_id}' đã được cập nhật thành công.",
        )
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Đã xảy ra lỗi nội bộ khi cập nhật khuôn mặt: {e}",
        )
