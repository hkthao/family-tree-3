using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Families.EventHandlers;

public class FamilyUpdatedEventHandler(ILogger<FamilyUpdatedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService, IUser user, IApplicationDbContext context) : INotificationHandler<FamilyUpdatedEvent>
{
    private readonly ILogger<FamilyUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly IUser _user = user;
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(FamilyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully updated.",
            notification.Family.Name, notification.Family.Id);

        if (!_user.Id.HasValue)
        {
            _logger.LogWarning("Current user ID not found when recording activity for family update. Activity will not be recorded.");
            return; // Or throw an exception, depending on desired behavior
        }

        // Record activity for family update
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = _user.Id.Value,
            ActionType = UserActionType.UpdateFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Updated family '{notification.Family.Name}'."
        }, cancellationToken);

        // Publish notification for family update
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Update family data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertEntityAsync(
            notification.Family,
            "Family",
            family => $"Family Name: {family.Name}. Description: {family.Description}. Address: {family.Address}",
            family => new Dictionary<string, string>
            {
                { "EntityType", "Family" },
                { "EntityId", family.Id.ToString() },
                { "Name", family.Name },
                { "Description", family.Description ?? "" },
                { "DeepLink", $"/family/{family.Id}" }
            },
            cancellationToken
        );
    }
}

