from typing import List, Optional
from uuid import UUID
from pydantic import BaseModel, Field

class BoundingBox(BaseModel):
    """
    Biểu diễn một hộp giới hạn xung quanh khuôn mặt được phát hiện.
    """
    x_min: float = Field(..., description="Tọa độ X tối thiểu của hộp giới hạn.")
    y_min: float = Field(..., description="Tọa độ Y tối thiểu của hộp giới hạn.")
    x_max: float = Field(..., description="Tọa độ X tối đa của hộp giới hạn.")
    y_max: float = Field(..., description="Tọa độ Y tối đa của hộp giới hạn.")

class FaceMetadata(BaseModel):
    """
    Metadata cho một khuôn mặt, phản ánh cấu trúc của MemberFace.
    """
    family_id: UUID = Field(..., description="ID của dòng họ mà khuôn mặt này thuộc về.")
    member_id: UUID = Field(..., description="ID của thành viên sở hữu khuôn mặt này.")
    face_id: Optional[UUID] = Field(None, description="ID duy nhất của khuôn mặt. Nếu không cung cấp, sẽ được tạo tự động.")
    
    bounding_box: Optional[BoundingBox] = Field(None, description="Hộp giới hạn của khuôn mặt trong ảnh gốc.")
    confidence: Optional[float] = Field(None, description="Độ tin cậy của việc phát hiện khuôn mặt.")

    thumbnail_url: Optional[str] = Field(None, description="URL của ảnh thumbnail khuôn mặt.")
    original_image_url: Optional[str] = Field(None, description="URL của ảnh gốc chứa khuôn mặt.")

    embedding: Optional[List[float]] = Field(None, description="Vector nhúng của khuôn mặt.")

    emotion: Optional[str] = Field(None, description="Cảm xúc chính được phát hiện.")
    emotion_confidence: Optional[float] = Field(None, description="Độ tin cậy của việc phát hiện cảm xúc.")

    vector_db_id: Optional[str] = Field(None, description="ID của vector trong cơ sở dữ liệu vector (LanceDB/Qdrant).")
    is_vector_db_synced: bool = Field(False, description="Đánh dấu xem thông tin khuôn mặt đã được đồng bộ hóa với cơ sở dữ liệu vector hay chưa.")

class AddFaceRequest(BaseModel):
    """
    Yêu cầu để thêm một khuôn mặt mới vào cơ sở dữ liệu.
    """
    face_metadata: FaceMetadata = Field(..., description="Metadata đầy đủ của khuôn mặt.")

class FaceEmbeddingResponse(BaseModel):
    """
    Phản hồi sau khi thêm hoặc cập nhật thông tin khuôn mặt.
    """
    face_id: UUID = Field(..., description="ID duy nhất của khuôn mặt.")
    member_id: UUID = Field(..., description="ID của thành viên liên quan.")
    message: str = Field(..., description="Thông báo về kết quả hoạt động.")
    score: Optional[float] = Field(None, description="Điểm số tương đồng khi tìm kiếm.")
    vector_db_id: Optional[str] = Field(None, description="ID của vector trong cơ sở dữ liệu vector (LanceDB/Qdrant).")

class UpdateFaceRequest(BaseModel):
    """
    Yêu cầu để cập nhật thông tin của một khuôn mặt hiện có.
    Các trường không được cung cấp sẽ không bị thay đổi.
    """
    member_id: Optional[UUID] = Field(None, description="ID của thành viên sở hữu khuôn mặt này.")
    bounding_box: Optional[BoundingBox] = Field(None, description="Hộp giới hạn của khuôn mặt trong ảnh gốc.")
    confidence: Optional[float] = Field(None, description="Độ tin cậy của việc phát hiện khuôn mặt.")
    thumbnail_url: Optional[str] = Field(None, description="URL của ảnh thumbnail khuôn mặt.")
    original_image_url: Optional[str] = Field(None, description="URL của ảnh gốc chứa khuôn mặt.")
    embedding: Optional[List[float]] = Field(None, description="Vector nhúng của khuôn mặt.")
    emotion: Optional[str] = Field(None, description="Cảm xúc chính được phát hiện.")
    emotion_confidence: Optional[float] = Field(None, description="Độ tin cậy của việc phát hiện cảm xúc.")
    vector_db_id: Optional[str] = Field(None, description="ID của vector trong cơ sở dữ liệu vector (LanceDB/Qdrant).")
    is_vector_db_synced: Optional[bool] = Field(None, description="Đánh dấu xem thông tin khuôn mặt đã được đồng bộ hóa với cơ sở dữ liệu vector hay chưa.")

class SearchFaceRequest(BaseModel):
    """
    Yêu cầu để tìm kiếm các khuôn mặt tương tự.
    """
    query_embedding: Optional[List[float]] = Field(None, description="Vector nhúng dùng để truy vấn tìm kiếm.")
    image_url: Optional[str] = Field(None, description="URL của hình ảnh để tạo vector nhúng truy vấn.")
    family_id: UUID = Field(..., description="ID của dòng họ để giới hạn phạm vi tìm kiếm.")
    member_id: Optional[UUID] = Field(None, description="ID thành viên để lọc kết quả tìm kiếm (tùy chọn).")
    top_k: int = Field(5, description="Số lượng kết quả hàng đầu muốn lấy.", gt=0)

class SearchResult(BaseModel):
    """
    Kết quả từ một truy vấn tìm kiếm khuôn mặt.
    """
    face_id: UUID = Field(..., description="ID của khuôn mặt được tìm thấy.")
    member_id: UUID = Field(..., description="ID của thành viên sở hữu khuôn mặt.")
    score: float = Field(..., description="Điểm số tương đồng với vector truy vấn.")
    metadata: Optional[FaceMetadata] = Field(None, description="Metadata đầy đủ của khuôn mặt được tìm thấy.")

class SearchFacesResponse(BaseModel):
    """
    Phản hồi cho yêu cầu tìm kiếm khuôn mặt.
    """
    results: List[SearchResult] = Field(..., description="Danh sách các khuôn mặt được tìm thấy.")