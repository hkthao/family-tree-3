using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

/// <summary>
/// Xử lý sự kiện khi một thành viên gia đình mới được thêm vào.
/// </summary>
public class NewFamilyMemberAddedEventHandler : INotificationHandler<NewFamilyMemberAddedEvent>
{
    private readonly ILogger<NewFamilyMemberAddedEventHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUser _user;

    public NewFamilyMemberAddedEventHandler(
        ILogger<NewFamilyMemberAddedEventHandler> logger,
        INotificationService notificationService,
        IUser user)
    {
        _logger = logger;
        _notificationService = notificationService;
        _user = user;
    }

    /// <summary>
    /// Xử lý sự kiện NewFamilyMemberAddedEvent.
    /// </summary>
    /// <param name="notification">Sự kiện NewFamilyMemberAddedEvent.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task Handle(NewFamilyMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FamilyTree Domain Event: {DomainEvent}", notification.GetType().Name);

        // Gửi thông báo cho người tạo thành viên mới hoặc các thành viên liên quan
        var message = new NotificationMessage
        {
            RecipientUserId = _user.Id ?? throw new InvalidOperationException("User ID not found."), // Gửi cho người dùng hiện tại
            SenderUserId = _user.Id,
            Title = "Thành viên mới được thêm vào gia đình",
            Message = $"Thành viên {notification.NewMember.FullName} đã được thêm vào gia đình của bạn.",
            Type = Domain.Enums.NotificationType.NewFamilyMember,
            FamilyId = notification.NewMember.FamilyId,
            PreferredChannels = new List<Domain.Enums.NotificationChannel> { Domain.Enums.NotificationChannel.InApp } // Chỉ gửi In-App
        };

        await _notificationService.SendNotification(message, cancellationToken);
    }
}
