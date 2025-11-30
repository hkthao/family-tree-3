using backend.Domain.Entities;

namespace backend.Domain.Events.MemberFaces;

/// <summary>
/// Sự kiện miền được kích hoạt khi một MemberFace bị xóa.
/// </summary>
public class MemberFaceDeletedEvent : BaseEvent
{
    /// <summary>
    /// Đối tượng MemberFace đã bị xóa.
    /// </summary>
    public MemberFace MemberFace { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của MemberFaceDeletedEvent.
    /// </summary>
    /// <param name="memberFace">Đối tượng MemberFace đã bị xóa.</param>
    public MemberFaceDeletedEvent(MemberFace memberFace)
    {
        MemberFace = memberFace;
    }
}