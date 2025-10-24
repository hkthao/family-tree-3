using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Families.EventHandlers;

public class FamilyDeletedEventHandler(ILogger<FamilyDeletedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<FamilyDeletedEvent>
{
    private readonly ILogger<FamilyDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(FamilyDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully deleted.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family deletion
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.DeleteFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Deleted family '{notification.Family.Name}'."
        }, cancellationToken);

        // Send notification for family deletion
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Family.LastModifiedBy!, // Assuming LastModifiedBy is the recipient
            Title = "Family Deleted",
            Body = $"Your family '{notification.Family.Name}' has been successfully deleted.",
            Data = new Dictionary<string, string>
            {
                { "FamilyId", notification.Family.Id.ToString() },
                { "FamilyName", notification.Family.Name }
            },
            DeepLink = $"/families" // No specific family page after deletion
        }, cancellationToken);

        // Remove family data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Family.Id.ToString(), "Family", cancellationToken);
    }
}
