using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Events;
using Microsoft.Extensions.Logging;
using backend.Application.Families.Commands.GenerateFamilyKb;

namespace backend.Application.Events.EventHandlers;

public class EventCreatedEventHandler(ILogger<EventCreatedEventHandler> logger, IMediator mediator, ICurrentUser user, IN8nService n8nService) : INotificationHandler<EventCreatedEvent>
{
    private readonly ILogger<EventCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = user;
    private readonly IN8nService _n8nService = n8nService;

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
       // await _mediator.Send(new GenerateFamilyKbCommand(notification.Event.FamilyId?.ToString() ?? string.Empty, notification.Event.Id.ToString(), KbRecordType.Event), cancellationToken);
    }
}
