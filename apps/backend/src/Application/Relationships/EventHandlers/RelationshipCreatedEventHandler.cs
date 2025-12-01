using backend.Application.Common.Helpers;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;
using Microsoft.Extensions.Logging;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipCreatedEventHandler(ILogger<RelationshipCreatedEventHandler> logger, IMediator mediator, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<RelationshipCreatedEvent>
{
    private readonly ILogger<RelationshipCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

    public async Task Handle(RelationshipCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully created.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship creation
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.CreateRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Created relationship between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId} (Type: {notification.Relationship.Type})."
        }, cancellationToken);


        // Store relationship data in Vector DB for search via GlobalSearchService
    }
}
