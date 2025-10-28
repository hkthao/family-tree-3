using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;
using Novu; // For NovuSDK
using Novu.Models.Components; // For DTOs like TriggerEventRequestDto, To

namespace backend.Infrastructure.Novu;

/// <summary>
/// Triển khai INotificationProvider sử dụng dịch vụ Novu để gửi thông báo.
/// </summary>
public class NovuNotificationProvider : INotificationProvider
{
    private readonly NovuSDK _novuSdk;
    private readonly NovuSettings _novuSettings;

    /// <summary>
    /// Khởi tạo một phiên bản mới của NovuNotificationProvider.
    /// </summary>
    /// <param name="novuSdk">SDK Novu để tương tác với API Novu.</param>
    /// <param name="novuSettings">Cài đặt cấu hình cho Novu.</param>
    public NovuNotificationProvider(NovuSDK novuSdk, IOptions<NovuSettings> novuSettings)
    {
        _novuSdk = novuSdk;
        _novuSettings = novuSettings.Value;
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

        // The Novu SDK example uses TriggerAsync directly on the sdk object.
        // It also uses TriggerEventRequestDto and To.CreateStr for the recipient.
        // The metadata can be passed as Payload.

        // Note: The Novu SDK example does not show a direct "Create Subscriber" method.
        // It assumes the subscriber already exists or is created implicitly by triggering an event.
        // If explicit subscriber creation is needed, I'll need to find the correct method.
        // For now, I'll proceed with TriggerAsync as shown in the example.

        var triggerEventRequestDto = new TriggerEventRequestDto()
        {
            WorkflowId = message.Title, // Using message.Title as WorkflowId
            Payload = message.Metadata, // Pass metadata as payload
            To = To.CreateStr(subscriberId), // Use To.CreateStr for subscriberId
        };

        await _novuSdk.TriggerAsync(triggerEventRequestDto: triggerEventRequestDto);
    }
}
