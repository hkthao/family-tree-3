using backend.Application.Common.Interfaces;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Members.EventHandlers;

public class MemberDeletedEventHandler(ILogger<MemberDeletedEventHandler> logger, IMediator mediator, IDomainEventNotificationPublisher notificationPublisher, IGlobalSearchService globalSearchService) : INotificationHandler<MemberDeletedEvent>
{
    private readonly ILogger<MemberDeletedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IDomainEventNotificationPublisher _notificationPublisher = notificationPublisher;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(MemberDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully deleted.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member deletion
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.DeleteMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Deleted member '{notification.Member.FullName}' from family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member deletion
        await _notificationPublisher.PublishNotificationForEventAsync(notification, cancellationToken);

        // Remove member data from Vector DB for search via GlobalSearchService
        await _globalSearchService.DeleteEntityFromSearchAsync(notification.Member.Id.ToString(), "Member", cancellationToken);
    }
}
