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
}
