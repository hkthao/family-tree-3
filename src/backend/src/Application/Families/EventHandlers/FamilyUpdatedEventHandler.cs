using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Families.EventHandlers;

public class FamilyUpdatedEventHandler(ILogger<FamilyUpdatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<FamilyUpdatedEvent>
{
    private readonly ILogger<FamilyUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(FamilyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully updated.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family update
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.UpdateFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Updated family '{notification.Family.Name}'."
        }, cancellationToken);

        // Send notification for family update
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Family.LastModifiedBy!, // Assuming LastModifiedBy is the recipient
            Title = "Family Updated",
            Body = $"Your family '{notification.Family.Name}' has been successfully updated.",
            Data = new Dictionary<string, string>
            {
                { "FamilyId", notification.Family.Id.ToString() },
                { "FamilyName", notification.Family.Name }
            },
            DeepLink = $"/families/{notification.Family.Id}" // Example deep link
        }, cancellationToken);

        // Update family data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertFamilyForSearchAsync(notification.Family, cancellationToken);
    }
}
