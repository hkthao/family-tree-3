using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipUpdatedEventHandler(ILogger<RelationshipUpdatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<RelationshipUpdatedEvent>
{
    private readonly ILogger<RelationshipUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(RelationshipUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully updated.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship update
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.UpdateRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Updated relationship {notification.Relationship.SourceMemberId}-{notification.Relationship.Type}-{notification.Relationship.TargetMemberId}."
        }, cancellationToken);

        // Send notification for relationship update
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Relationship.LastModifiedBy?.ToString() ?? "UnknownUser", // Use LastModifiedBy from auditable entity
            Title = "Relationship Updated",
            Body = $"Relationship (Type: {notification.Relationship.Type}) between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId} has been successfully updated.",
            Data = new Dictionary<string, string>
            {
                { "RelationshipId", notification.Relationship.Id.ToString() },
                { "RelationshipType", notification.Relationship.Type.ToString() },
                { "SourceMemberId", notification.Relationship.SourceMemberId.ToString() },
                { "TargetMemberId", notification.Relationship.TargetMemberId.ToString() }
            },
            DeepLink = $"/relationships/{notification.Relationship.Id}" // Example deep link
        }, cancellationToken);
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
                { "DeepLink", $"/relationships/{relationship.Id}" }
            },
            cancellationToken
        );
    }
}
