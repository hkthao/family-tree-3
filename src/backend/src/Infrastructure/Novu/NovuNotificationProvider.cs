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
    /// <param name="novuSettings">Cài đặt cấu hình cho Novu.</param>
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
        var res = await _novuSdk.Subscribers.RetrieveAsync(subscriberId: subscriberId);
        if (res.HttpMeta.Response.StatusCode == HttpStatusCode.NotFound)
        {
            // Extract subscriber details from metadata
            var firstName = message.Metadata?.GetValueOrDefault("FirstName", "")?.ToString();
            var lastName = message.Metadata?.GetValueOrDefault("LastName", "")?.ToString();
            var email = message.Metadata?.GetValueOrDefault("Email", "")?.ToString();
            var phone = message.Metadata?.GetValueOrDefault("Phone", "")?.ToString();

            await _novuSdk.Subscribers.CreateAsync(createSubscriberRequestDto: new CreateSubscriberRequestDto()
            {
                SubscriberId = subscriberId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
            });
        }

        var triggerEventRequestDto = new TriggerEventRequestDto()
        {
            WorkflowId = message.Title, // Using message.Title as WorkflowId
            Payload = message.Metadata, // Pass metadata as payload
            To = To.CreateStr(subscriberId), // Use To.CreateStr for subscriberId
        };

        await _novuSdk.TriggerAsync(triggerEventRequestDto: triggerEventRequestDto);
    }
}