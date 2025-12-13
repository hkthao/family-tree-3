using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.GenerateFamilyKb;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.EventHandlers;

public class FamilyUpdatedEventHandler(ILogger<FamilyUpdatedEventHandler> logger, IMediator mediator, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<FamilyUpdatedEvent>
{
    private readonly ILogger<FamilyUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

    public async Task Handle(FamilyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully updated.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family update
        if (_user.UserId != Guid.Empty)
        {
            await _mediator.Send(new RecordActivityCommand
            {
                UserId = _user.UserId,
                ActionType = UserActionType.UpdateFamily,
                TargetType = TargetType.Family,
                TargetId = notification.Family.Id.ToString(),
                ActivitySummary = $"Updated family '{notification.Family.Name}'."
            }, cancellationToken);

            // Publish notification for family update
            await _mediator.Send(new GenerateFamilyKbCommand(notification.Family.Id.ToString(), notification.Family.Id.ToString(), KbRecordType.Family), cancellationToken);
        }
    }
}

