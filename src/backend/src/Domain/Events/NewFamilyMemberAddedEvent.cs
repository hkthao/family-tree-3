using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

/// <summary>
/// Sự kiện miền được kích hoạt khi một thành viên gia đình mới được thêm vào.
/// </summary>
public class NewFamilyMemberAddedEvent : BaseEvent, IDomainEvent
{
    /// <summary>
    /// Thành viên gia đình mới được thêm vào.
    /// </summary>
    public Member NewMember { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="NewFamilyMemberAddedEvent"/>.
    /// </summary>
    /// <param name="newMember">Thành viên gia đình mới được thêm vào.</param>
    public NewFamilyMemberAddedEvent(Member newMember)
    {
        NewMember = newMember;
    }
}
