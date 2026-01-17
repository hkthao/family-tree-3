using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.GenerateFamilyKb;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.EventHandlers;

public class FamilyCreatedEventHandler(ILogger<FamilyCreatedEventHandler> logger, IMediator mediator, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<FamilyCreatedEvent>
{
    private readonly ILogger<FamilyCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

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
                            ActivitySummary = $"Đã tạo gia đình '{notification.Family.Name}'."        }, cancellationToken);

        // Publish notification for family creation
        await _mediator.Send(new GenerateFamilyKbCommand(notification.Family.Id.ToString(), notification.Family.Id.ToString(), KbRecordType.Family), cancellationToken);
    }
}

