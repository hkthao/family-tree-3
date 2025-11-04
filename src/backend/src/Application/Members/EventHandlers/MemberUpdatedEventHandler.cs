using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberUpdatedEventHandler(ILogger<MemberUpdatedEventHandler> logger, IMediator mediator, IGlobalSearchService globalSearchService, ICurrentUser _user) : INotificationHandler<MemberUpdatedEvent>
{
    private readonly ILogger<MemberUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = _user;

    public async Task Handle(MemberUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully updated.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member update
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.ProfileId!.Value,
            ActionType = UserActionType.UpdateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Updated member '{notification.Member.FullName}' in family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member update

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
                { "DeepLink", $"/family/{member.FamilyId}/member/{member.Id}" }
            },
            cancellationToken
        );
    }
}
