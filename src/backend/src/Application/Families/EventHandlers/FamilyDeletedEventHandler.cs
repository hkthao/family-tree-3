using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Families.EventHandlers;

public class FamilyDeletedEventHandler(ILogger<FamilyDeletedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService,IUser _user) : INotificationHandler<FamilyDeletedEvent>
{
    private readonly ILogger<FamilyDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly IUser _user = _user;

    public async Task Handle(FamilyDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully deleted.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family deletion
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = _user.Id!.Value,
            ActionType = UserActionType.DeleteFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Deleted family '{notification.Family.Name}'."
        }, cancellationToken);

        // Publish notification for family deletion
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Remove family data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Family.Id.ToString(), "Family", cancellationToken);
    }
}

