using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Events.EventHandlers;

public class EventDeletedEventHandler(ILogger<EventDeletedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService, IUser _user) : INotificationHandler<EventDeletedEvent>
{
    private readonly ILogger<EventDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly IUser _user = _user;

    public async Task Handle(EventDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Event {EventName} ({EventId}) was successfully deleted.",
            notification.Event.Name, notification.Event.Id);

        // Record activity for event deletion
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = _user.Id!.Value,
            ActionType = UserActionType.DeleteEvent,
            TargetType = TargetType.Event,
            TargetId = notification.Event.Id.ToString(),
            ActivitySummary = $"Deleted event '{notification.Event.Name}'."
        }, cancellationToken);

        // Publish notification for event deletion
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Remove event data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Event.Id.ToString(), "Event", cancellationToken);
    }
}
