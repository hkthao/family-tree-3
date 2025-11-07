using backend.Application.Common.Interfaces;
using backend.Application.Common.Helpers;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;
using Microsoft.Extensions.Logging;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipUpdatedEventHandler(ILogger<RelationshipUpdatedEventHandler> logger, IMediator mediator, IGlobalSearchService globalSearchService, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<RelationshipUpdatedEvent>
{
    private readonly ILogger<RelationshipUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

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
            ActivitySummary = $"Updated relationship {notification.Relationship.SourceMemberId}-{notification.Relationship.Type}-{notification.Relationship.TargetMemberId}."
        }, cancellationToken);

        // Publish notification for relationship update

        // Update relationship data in Vector DB for search via GlobalSearchService
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

        // Call n8n webhook for embedding update
        var (entityData, description) = EmbeddingDescriptionFactory.CreateRelationshipData(notification.Relationship);
        var embeddingDto = new EmbeddingWebhookDto
        {
            EntityType = "Relationship",
            EntityId = notification.Relationship.Id.ToString(),
            ActionType = "Updated",
            EntityData = entityData,
            Description = description
        };
        await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);
    }
}
