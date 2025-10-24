using backend.Application.Services;
using backend.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backend.Web.Services;

/// <summary>
/// Triển khai INotificationHubService sử dụng SignalR Hub.
/// </summary>
public class SignalRNotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gửi một thông báo đến một người dùng cụ thể thông qua SignalR.
    /// </summary>
    /// <param name="userId">ID của người dùng nhận thông báo.</param>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="message">Nội dung thông báo.</param>
    /// <param name="type">Loại thông báo (chuỗi).</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendNotificationToUserAsync(string userId, string title, string message, string type, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message, type, cancellationToken);
    }
}
