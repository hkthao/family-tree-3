using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.EventHandlers;

public class FamilyDeletedEventHandler(ILogger<FamilyDeletedEventHandler> logger, IMediator mediator, ICurrentUser _user) : INotificationHandler<FamilyDeletedEvent>
{
    private readonly ILogger<FamilyDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;

    public async Task Handle(FamilyDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully deleted.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family deletion
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.DeleteFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Deleted family '{notification.Family.Name}'."
        }, cancellationToken);

        // Publish notification for family deletion

        // Remove family data from Vector DB for search via GlobalSearchService
        _logger.LogInformation("Family Deleted: {FamilyId}", notification.Family.Id);
    }
}

