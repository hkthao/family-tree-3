using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Relationships;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Application.Relationships.EventHandlers;

public class RelationshipDeletedEventHandler(ILogger<RelationshipDeletedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<RelationshipDeletedEvent>
{
    private readonly ILogger<RelationshipDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(RelationshipDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Relationship {RelationshipId} (Type: {RelationshipType}) was successfully deleted.",
            notification.Relationship.Id, notification.Relationship.Type);

        // Record activity for relationship deletion
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.DeleteRelationship,
            TargetType = TargetType.Relationship,
            TargetId = notification.Relationship.Id.ToString(),
            ActivitySummary = $"Deleted relationship {notification.Relationship.SourceMemberId}-{notification.Relationship.Type}-{notification.Relationship.TargetMemberId}."
        }, cancellationToken);

        // Send notification for relationship deletion
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Relationship.LastModifiedBy?.ToString() ?? "UnknownUser", // Use LastModifiedBy from auditable entity
            Title = "Relationship Deleted",
            Message = $"Relationship (Type: {notification.Relationship.Type}) between {notification.Relationship.SourceMemberId} and {notification.Relationship.TargetMemberId} has been successfully deleted.",
            Data = System.Text.Json.JsonSerializer.Serialize(new
            {
                RelationshipId = notification.Relationship.Id.ToString(),
                RelationshipType = notification.Relationship.Type.ToString(),
                SourceMemberId = notification.Relationship.SourceMemberId.ToString(),
                TargetMemberId = notification.Relationship.TargetMemberId.ToString(),
                DeepLink = "/relationships"
            })
        }, cancellationToken);

        // Remove relationship data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Relationship.Id.ToString(), "Relationship", cancellationToken);
    }
}
