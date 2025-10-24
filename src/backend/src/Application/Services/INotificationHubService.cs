namespace backend.Application.Services;

/// <summary>
/// Giao diện cho dịch vụ tương tác với SignalR Hub để gửi thông báo theo thời gian thực.
/// </summary>
public interface INotificationHubService
{
    /// <summary>
    /// Gửi một thông báo đến một người dùng cụ thể thông qua SignalR.
    /// </summary>
    /// <param name="userId">ID của người dùng nhận thông báo.</param>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="message">Nội dung thông báo.</param>
    /// <param name="type">Loại thông báo (chuỗi).</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendNotificationToUserAsync(string userId, string title, string message, string type, CancellationToken cancellationToken = default);
}
