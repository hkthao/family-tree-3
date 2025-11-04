using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberDeletedEventHandler(ILogger<MemberDeletedEventHandler> logger, IMediator mediator, IGlobalSearchService globalSearchService, ICurrentUser _user) : INotificationHandler<MemberDeletedEvent>
{
    private readonly ILogger<MemberDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;
    private readonly ICurrentUser _user = _user;

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
            ActivitySummary = $"Deleted member '{notification.Member.FullName}' from family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member deletion

        // Remove member data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Member.Id.ToString(), "Member", cancellationToken);
    }
}
