using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Relationships;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipCreatedEventHandler(ILogger<RelationshipCreatedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService,IUser _user) : INotificationHandler<RelationshipCreatedEvent>
{
    private readonly ILogger<RelationshipCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly IUser _user = _user;

    public async Task Handle(RelationshipCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully created.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship creation
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = _user.Id!.Value,
            ActionType = UserActionType.CreateRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Created relationship between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId} (Type: {notification.Relationship.Type})."
        }, cancellationToken);

        // Publish notification for relationship creation
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Store relationship data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertEntityAsync(
            notification.Relationship,
            "Relationship",
            relationship => $"Relationship Type: {relationship.Type}. Source Member: {relationship.SourceMemberId}. Target Member: {relationship.TargetMemberId}.",
            relationship => new Dictionary<string, string>
            {
                { "EntityType", "Relationship" },
                { "EntityId", relationship.Id.ToString() },
                { "Type", relationship.Type.ToString() },
                { "SourceMemberId", relationship.SourceMemberId.ToString() },
                { "TargetMemberId", relationship.TargetMemberId.ToString() },
                { "DeepLink", $"/relationship/{relationship.Id}" }
            },
            cancellationToken
        );
    }
}
