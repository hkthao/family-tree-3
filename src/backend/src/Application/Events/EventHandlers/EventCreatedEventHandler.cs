using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Events.EventHandlers;

public class EventCreatedEventHandler(ILogger<EventCreatedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService, ICurrentUser user) : INotificationHandler<EventCreatedEvent>
{
    private readonly ILogger<EventCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = user;

    public async Task Handle(EventCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Event {EventName} ({EventId}) was successfully created.",
            notification.Event.Name, notification.Event.Id);

        // Record activity for event creation
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.CreateEvent,
            TargetType = TargetType.Event,
            TargetId = notification.Event.Id.ToString(),
            ActivitySummary = $"Created event '{notification.Event.Name}'."
        }, cancellationToken);

        // Publish notification for event creation
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Store event data in Vector DB for search via GlobalSearchService
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
