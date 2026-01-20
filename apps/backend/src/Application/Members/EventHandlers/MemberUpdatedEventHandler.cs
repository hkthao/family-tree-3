using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberUpdatedEventHandler(ILogger<MemberUpdatedEventHandler> logger, IMediator mediator, IFamilyTreeService familyTreeService, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<MemberUpdatedEvent>
{
    private readonly ILogger<MemberUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

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
            ActivitySummary = $"Đã cập nhật thành viên '{notification.Member.FullName}' trong gia đình '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member update

        // Sync member life events
        await _familyTreeService.SyncMemberLifeEvents(
            notification.Member.Id,
            notification.Member.FamilyId,
            notification.Member.DateOfBirth.HasValue ? DateOnly.FromDateTime(notification.Member.DateOfBirth.Value) : (DateOnly?)null,
            notification.Member.DateOfDeath.HasValue ? DateOnly.FromDateTime(notification.Member.DateOfDeath.Value) : (DateOnly?)null,
            notification.Member.FullName,
            cancellationToken
        );

        // Update family stats
        await _familyTreeService.UpdateFamilyStats(notification.Member.FamilyId, cancellationToken);
    }
}
