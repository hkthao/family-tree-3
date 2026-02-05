using backend.Application.Common.Interfaces.Core;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;
using Microsoft.Extensions.Logging;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipUpdatedEventHandler(ILogger<RelationshipUpdatedEventHandler> logger, IMediator mediator, ICurrentUser _user) : INotificationHandler<RelationshipUpdatedEvent>
{
    private readonly ILogger<RelationshipUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;

    public async Task Handle(RelationshipUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully updated.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship update
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.UpdateRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Đã cập nhật mối quan hệ {notification.Relationship.SourceMemberId}-{notification.Relationship.Type}-{notification.Relationship.TargetMemberId}."
        }, cancellationToken);

        // Publish notification for relationship update
    }
}
