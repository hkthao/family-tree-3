using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipCreatedEventHandler(ILogger<RelationshipCreatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<RelationshipCreatedEvent>
{
    private readonly ILogger<RelationshipCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(RelationshipCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully created.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship creation
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.CreateRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Created relationship between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId} (Type: {notification.Relationship.Type})."
        }, cancellationToken);

        // Send notification for relationship creation
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Relationship.CreatedBy?.ToString() ?? "UnknownUser", // Use CreatedBy from auditable entity
            Title = "Relationship Created",
            Body = $"A new relationship (Type: {notification.Relationship.Type}) has been created between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId}.",
            Data = new Dictionary<string, string>
            {
                { "RelationshipId", notification.Relationship.Id.ToString() },
                { "RelationshipType", notification.Relationship.Type.ToString() },
                { "SourceMemberId", notification.Relationship.SourceMemberId.ToString() },
                { "TargetMemberId", notification.Relationship.TargetMemberId.ToString() }
            },
            DeepLink = $"/relationships/{notification.Relationship.Id}" // Example deep link
        }, cancellationToken);

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
                { "DeepLink", $"/relationships/{relationship.Id}" }
            },
            cancellationToken
        );
    }
}
