using backend.Application.Common.Interfaces.Core;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.EventHandlers;

public class EventDeletedEventHandler(ILogger<EventDeletedEventHandler> logger, IMediator mediator, ICurrentUser _user) : INotificationHandler<EventDeletedEvent>
{
    private readonly ILogger<EventDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;

    public async Task Handle(EventDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Event {EventName} ({EventId}) was successfully deleted.",
            notification.Event.Name, notification.Event.Id);

        // Record activity for event deletion
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.DeleteEvent,
            TargetType = TargetType.Event,
            TargetId = notification.Event.Id.ToString(),
            ActivitySummary = $"Đã xóa sự kiện '{notification.Event.Name}'."
        }, cancellationToken);

        // Publish notification for event deletion

        // Remove event data from Vector DB for search via GlobalSearchService
        _logger.LogInformation("Event Deleted: {EventId}", notification.Event.Id);
    }
}
