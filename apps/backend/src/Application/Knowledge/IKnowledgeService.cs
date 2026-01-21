using backend.Application.Knowledge.DTOs; // Added for KnowledgeSearchResultDto

namespace backend.Application.Knowledge;

public interface IKnowledgeService
{
    /// <summary>
    /// Index (hoặc cập nhật) dữ liệu của một Family vào knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task IndexFamilyData(Guid familyId);

    /// <summary>
    /// Index (hoặc cập nhật) dữ liệu của một Member vào knowledge-search-service.
    /// </summary>
    /// <param name="memberId">ID của Member.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task IndexMemberData(Guid memberId);

    /// <summary>
    /// Index (hoặc cập nhật) dữ liệu của một Event vào knowledge-search-service.
    /// </summary>
    /// <param name="eventId">ID của Event.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task IndexEventData(Guid eventId);

    /// <summary>
    /// Upsert (thêm mới hoặc cập nhật) dữ liệu của một Family vào knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task UpsertFamilyData(Guid familyId);

    /// <summary>
    /// Upsert (thêm mới hoặc cập nhật) dữ liệu của một Member vào knowledge-search-service.
    /// </summary>
    /// <param name="memberId">ID của Member.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task UpsertMemberData(Guid memberId);

    /// <summary>
    /// Upsert (thêm mới hoặc cập nhật) dữ liệu của một Event vào knowledge-search-service.
    /// </summary>
    /// <param name="eventId">ID của Event.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task UpsertEventData(Guid eventId);

    /// <summary>
    /// Xóa dữ liệu của một Family khỏi knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteFamilyData(Guid familyId);

    /// <summary>
    /// Xóa dữ liệu của một Member khỏi knowledge-search-service.
    /// </summary>
    /// <param name="memberId">ID của Member.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteMemberData(Guid memberId);

    /// <summary>
    /// Xóa dữ liệu của một Event khỏi knowledge-search-service.
    /// </summary>
    /// <param name="eventId">ID của Event.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteEventData(Guid eventId);

    /// <summary>
    /// Xóa tất cả dữ liệu kiến thức của một Family khỏi knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteKnowledgeByFamilyId(Guid familyId);

    /// <summary>
    /// Tìm kiếm trong knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family để giới hạn phạm vi tìm kiếm.</param>
    /// <param name="queryString">Chuỗi truy vấn.</param>
    /// <param name="topK">Số lượng kết quả hàng đầu mong muốn.</param>
    /// <param name="allowedVisibility">Danh sách các mức độ hiển thị được phép.</param>
    /// <returns>Danh sách các KnowledgeSearchResultDto phù hợp.</returns>
    Task<List<KnowledgeSearchResultDto>> SearchKnowledgeBase(Guid familyId, string queryString, int topK, List<string> allowedVisibility);

    /// <summary>
    /// Index (hoặc cập nhật) dữ liệu của một khuôn mặt vào knowledge-search-service.
    /// </summary>
    /// <param name="faceData">Dữ liệu khuôn mặt để index/cập nhật.</param>
    /// <returns>VectorDbId của khuôn mặt đã được index.</returns>
    Task<string> IndexMemberFaceData(MemberFaceDto faceData);

    /// <summary>
    /// Xóa dữ liệu của một khuôn mặt cụ thể khỏi knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family mà khuôn mặt thuộc về.</param>
    /// <param name="faceId">ID của khuôn mặt cần xóa.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteMemberFaceData(Guid familyId, Guid faceId);

    /// <summary>
    /// Xóa tất cả dữ liệu khuôn mặt của một thành viên khỏi knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family mà thành viên thuộc về.</param>
    /// <param name="memberId">ID của thành viên có khuôn mặt cần xóa.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteMemberFacesByMemberId(Guid familyId, Guid memberId);

    /// <summary>
    /// Xóa tất cả dữ liệu khuôn mặt của một gia đình khỏi knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family mà các khuôn mặt thuộc về.</param>
    /// <returns>Task hoàn thành tác vụ.</returns>
    Task DeleteFamilyFacesData(Guid familyId);

    /// <summary>
    /// Tìm kiếm khuôn mặt tương tự trong knowledge-search-service.
    /// </summary>
    /// <param name="familyId">ID của Family để giới hạn phạm vi tìm kiếm.</param>
    /// <param name="embedding">Vector nhúng của khuôn mặt truy vấn.</param>
    /// <param name="memberId">Tùy chọn: ID của thành viên để lọc kết quả tìm kiếm.</param>
    /// <param name="topK">Số lượng kết quả hàng đầu mong muốn.</param>
    /// <returns>Danh sách các FaceSearchResultDto phù hợp.</returns>
    Task<List<FaceSearchResultDto>> SearchFaces(Guid familyId, List<double> embedding, Guid? memberId = null, int topK = 5);

}
