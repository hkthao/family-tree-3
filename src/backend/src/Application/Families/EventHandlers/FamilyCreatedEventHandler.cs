using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.EventHandlers;

public class FamilyCreatedEventHandler(ILogger<FamilyCreatedEventHandler> logger, IMediator mediator, IGlobalSearchService globalSearchService, ICurrentUser _user) : INotificationHandler<FamilyCreatedEvent>
{
    private readonly ILogger<FamilyCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = _user;

    public async Task Handle(FamilyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully created.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family creation
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.CreateFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Created family '{notification.Family.Name}'."
        }, cancellationToken);

        // Publish notification for family creation

        // Store family data in Vector DB for search via GlobalSearchService
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

