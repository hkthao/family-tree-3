using backend.Application.Common.Helpers;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger, IMediator mediator, IFamilyTreeService familyTreeService, ICurrentUser _user, IN8nService n8nService, IStringLocalizer<MemberCreatedEventHandler> localizer, IApplicationDbContext context) : INotificationHandler<MemberCreatedEvent>
{
    private readonly ILogger<MemberCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;
    private readonly IStringLocalizer<MemberCreatedEventHandler> _localizer = localizer;
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(MemberCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully created.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member creation
        var family = await _context.Families.FindAsync(new object[] { notification.Member.FamilyId }, cancellationToken);
        var familyName = family?.Name ?? notification.Member.FamilyId.ToString(); // Fallback to ID if name not found

        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.CreateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = _localizer["Created member '{0}' in family '{1}'", notification.Member.FullName, familyName]
        }, cancellationToken);

        // Publish notification for member creation
    }
}
