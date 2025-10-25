using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Members.EventHandlers;

public class MemberUpdatedEventHandler(ILogger<MemberUpdatedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService) : INotificationHandler<MemberUpdatedEvent>
{
    private readonly ILogger<MemberUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(MemberUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully updated.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member update
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.UpdateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Updated member '{notification.Member.FullName}' in family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member update
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Update member data in Vector DB for search via GlobalSearchService
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
                { "DeepLink", $"/families/{member.FamilyId}/member/{member.Id}" }
            },
            cancellationToken
        );
    }
}
