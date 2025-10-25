using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Members.EventHandlers;

public class MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService, IFamilyTreeService familyTreeService) : INotificationHandler<MemberCreatedEvent>
{
    private readonly ILogger<MemberCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task Handle(MemberCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully created.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member creation
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.CreateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Created member '{notification.Member.FullName}' in family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member creation
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Store member data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertEntityAsync(
            notification.Member,
            "Member",
            member => $"Member Name: {member.FullName}. Biography: {member.Biography}. Occupation: {member.Occupation}. Place of birth: {member.PlaceOfBirth}.",
            member => new Dictionary<string, string>
            {
                { "EntityType", "Member" },
                { "EntityId", member.Id.ToString() },
                { "Name", member.FullName },
                { "Description", member.Biography ?? "" },
                { "DeepLink", $"/family/{member.FamilyId}/member/{member.Id}" }
            },
            cancellationToken
        );

        // Update family stats
        await _familyTreeService.UpdateFamilyStats(notification.Member.FamilyId, cancellationToken);
    }
}
