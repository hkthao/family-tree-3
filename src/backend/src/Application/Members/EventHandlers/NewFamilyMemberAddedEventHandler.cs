using backend.Application.Common.Interfaces;
using backend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

/// <summary>
/// Xử lý sự kiện khi một thành viên gia đình mới được thêm vào.
/// </summary>
public class NewFamilyMemberAddedEventHandler : INotificationHandler<NewFamilyMemberAddedEvent>
{
    private readonly ILogger<NewFamilyMemberAddedEventHandler> _logger;
    private readonly IDomainEventNotificationPublisher _notificationPublisher;

    public NewFamilyMemberAddedEventHandler(
        ILogger<NewFamilyMemberAddedEventHandler> logger,
        IDomainEventNotificationPublisher notificationPublisher)
    {
        _logger = logger;
        _notificationPublisher = notificationPublisher;
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

        // Publish notification for new family member added
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);
    }
}
