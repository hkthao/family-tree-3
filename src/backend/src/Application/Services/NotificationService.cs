using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;

namespace backend.Application.Services;

/// <summary>
/// Triển khai dịch vụ gửi thông báo, sử dụng các mẫu thông báo và các kênh gửi khác nhau.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationProviderFactory _notificationProviderFactory;
    private readonly NotificationSettings _notificationSettings;

    /// <summary>
    /// Khởi tạo một phiên bản mới của NotificationService.
    /// </summary>
    /// <param name="notificationProviderFactory">Factory để lấy nhà cung cấp thông báo.</param>
    /// <param name="notificationSettings">Cài đặt cấu hình cho dịch vụ thông báo.</param>
    public NotificationService(INotificationProviderFactory notificationProviderFactory, IOptions<NotificationSettings> notificationSettings)
    {
        _notificationProviderFactory = notificationProviderFactory;
        _notificationSettings = notificationSettings.Value;
    }

    /// <summary>
    /// Gửi một tin nhắn thông báo thông qua nhà cung cấp thông báo đã đăng ký.
    /// </summary>
    /// <param name="message">Tin nhắn thông báo chứa thông tin người nhận, nội dung và metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public Task SendNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_notificationSettings.Provider))
        {
            throw new InvalidOperationException("Notification provider is not configured.");
        }

        var provider = _notificationProviderFactory.GetProvider(_notificationSettings.Provider);
        return provider.SendAsync(message, cancellationToken);
    }
}
