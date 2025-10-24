using Microsoft.AspNetCore.SignalR;

namespace backend.Web.Hubs;

/// <summary>
/// SignalR Hub để gửi thông báo trong ứng dụng theo thời gian thực.
/// </summary>
public class NotificationHub : Hub
{
    /// <summary>
    /// Gửi một thông báo đến một người dùng cụ thể.
    /// </summary>
    /// <param name="userId">ID của người dùng nhận thông báo.</param>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="message">Nội dung thông báo.</param>
    /// <param name="type">Loại thông báo.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendNotificationToUser(string userId, string title, string message, string type)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", title, message, type);
    }

    /// <summary>
    /// Gửi một thông báo đến tất cả các client được kết nối.
    /// </summary>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="message">Nội dung thông báo.</param>
    /// <param name="type">Loại thông báo.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendNotificationToAll(string title, string message, string type)
    {
        await Clients.All.SendAsync("ReceiveNotification", title, message, type);
    }
}
