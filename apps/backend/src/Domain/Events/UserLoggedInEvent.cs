using System.Security.Claims;

namespace backend.Domain.Events;

/// <summary>
/// Sự kiện miền được kích hoạt khi người dùng đăng nhập thành công.
/// </summary>
public class UserLoggedInEvent : BaseEvent
{
    /// <summary>
    /// Thông tin chính của người dùng đã đăng nhập.
    /// </summary>
    public ClaimsPrincipal UserPrincipal { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của UserLoggedInEvent.
    /// </summary>
    /// <param name="userPrincipal">Thông tin chính của người dùng đã đăng nhập.</param>
    public UserLoggedInEvent(ClaimsPrincipal userPrincipal)
    {
        UserPrincipal = userPrincipal;
    }
}
