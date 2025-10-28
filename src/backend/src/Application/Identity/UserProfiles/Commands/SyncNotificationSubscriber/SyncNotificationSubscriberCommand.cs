using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Queries;

namespace backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;

/// <summary>
/// Lệnh để đồng bộ hóa thông tin người dùng hiện tại với dịch vụ thông báo đám mây.
/// </summary>
public record SyncNotificationSubscriberCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Thông tin chính của người dùng đã đăng nhập.
    /// </summary>
    public UserProfileDto UserProfile { get; init; } = null!;
}
