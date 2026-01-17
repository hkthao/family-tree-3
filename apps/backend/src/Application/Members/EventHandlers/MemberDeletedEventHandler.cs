using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberDeletedEventHandler(ILogger<MemberDeletedEventHandler> logger, IMediator mediator, ICurrentUser _user, IFamilyTreeService familyTreeService) : INotificationHandler<MemberDeletedEvent>
{
    private readonly ILogger<MemberDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task Handle(MemberDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully deleted.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member deletion
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.DeleteMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Đã xóa thành viên '{notification.Member.FullName}' khỏi gia đình '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member deletion

        // Remove member data from Vector DB for search via GlobalSearchService
        _logger.LogInformation("Member Deleted: {MemberId}", notification.Member.Id);

        // Update family stats
        await _familyTreeService.UpdateFamilyStats(notification.Member.FamilyId, cancellationToken);
    }
}
