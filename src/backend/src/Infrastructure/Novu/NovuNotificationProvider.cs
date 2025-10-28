using System.Net;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;
using Novu; // For NovuSDK
using Novu.Models.Components; // For DTOs like TriggerEventRequestDto, To, SubscriberCreateData

namespace backend.Infrastructure.Novu;

/// <summary>
/// Triển khai INotificationProvider sử dụng dịch vụ Novu để gửi thông báo.
/// </summary>
public class NovuNotificationProvider : INotificationProvider
{
    private readonly NovuSDK _novuSdk;

    /// <summary>
    /// Khởi tạo một phiên bản mới của NovuNotificationProvider.
    /// </summary>
    /// <param name="novuSdk">SDK Novu để tương tác với API Novu.</param>
    public NovuNotificationProvider(NovuSDK novuSdk)
    {
        _novuSdk = novuSdk;
    }

    /// <summary>
    /// Gửi một tin nhắn thông báo thông qua Novu.
    /// </summary>
    /// <param name="message">Tin nhắn thông báo chứa thông tin người nhận, nội dung và metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        // Novu yêu cầu một subscriberId. Sử dụng RecipientUserId làm subscriberId.
        var subscriberId = message.RecipientUserId;
        var triggerEventRequestDto = new TriggerEventRequestDto()
        {
            WorkflowId = message.Title, // Using message.Title as WorkflowId
            Payload = message.Metadata, // Pass metadata as payload
            To = To.CreateStr(subscriberId), // Use To.CreateStr for subscriberId
        };

        await _novuSdk.TriggerAsync(triggerEventRequestDto: triggerEventRequestDto);
    }

    /// <summary>
    /// Đồng bộ hóa thông tin người đăng ký với Novu.
    /// </summary>
    /// <param name="subscriberId">ID duy nhất của người đăng ký.</param>
    /// <param name="firstName">Tên của người đăng ký.</param>
    /// <param name="lastName">Họ của người đăng ký.</param>
    /// <param name="email">Địa chỉ email của người đăng ký.</param>
    /// <param name="phone">Số điện thoại của người đăng ký.</param>
    /// <param name="metadata">Dữ liệu bổ sung của người đăng ký.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SyncSubscriberAsync(
        string subscriberId,
        string? firstName,
        string? lastName,
        string? email,
        string? phone)
    {
        // Novu's SDK has a Create method for subscribers, which acts as an upsert.
        // It will create if not found, or update if found.
        await _novuSdk.Subscribers.CreateAsync(createSubscriberRequestDto: new CreateSubscriberRequestDto()
        {
            SubscriberId = subscriberId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            // You can add more subscriber data here if available in metadata
            // For example, if metadata contains "Avatar", you can add it here.
            // Data = metadata // If Novu's CreateSubscriberRequestDto supports a generic data field
        });
    }
}
