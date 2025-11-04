using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Events.EventHandlers;

public class EventUpdatedEventHandler(ILogger<EventUpdatedEventHandler> logger, IMediator mediator,  IGlobalSearchService globalSearchService, ICurrentUser user) : INotificationHandler<EventUpdatedEvent>
{
    private readonly ILogger<EventUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = user;

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

        // Update event data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertEntityAsync(
            notification.Event,
            "Event",
            @event => $"Event Name: {@event.Name}. Description: {@event.Description}. Location: {@event.Location}",
            @event => new Dictionary<string, string>
            {
                { "EntityType", "Event" },
                { "EntityId", @event.Id.ToString() },
                { "Name", @event.Name },
                { "Description", @event.Description ?? "" },
                { "DeepLink", $"/event/{@event.Id}" }
            },
            cancellationToken
        );
    }
}
