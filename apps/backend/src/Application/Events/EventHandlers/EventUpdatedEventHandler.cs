using backend.Application.Common.Helpers;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.EventHandlers;

public class EventUpdatedEventHandler(ILogger<EventUpdatedEventHandler> logger, IMediator mediator, ICurrentUser user, IN8nService n8nService) : INotificationHandler<EventUpdatedEvent>
{
    private readonly ILogger<EventUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = user;
    private readonly IN8nService _n8nService = n8nService;

    public async Task Handle(EventUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Event {EventName} ({EventId}) was successfully updated.",
            notification.Event.Name, notification.Event.Id);

        if (_user.UserId == Guid.Empty)
        {
            _logger.LogWarning("Current user ID not found when recording activity for event update. Activity will not be recorded.");
            return; // Or throw an exception, depending on desired behavior
        }

        // Record activity for event update
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.UpdateEvent,
            TargetType = TargetType.Event,
            TargetId = notification.Event.Id.ToString(),
            ActivitySummary = $"Updated event '{notification.Event.Name}'."
        }, cancellationToken);

        // Publish notification for event update
    }
}
