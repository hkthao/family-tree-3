using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.EventHandlers;

public class FamilyCreatedEventHandler(ILogger<FamilyCreatedEventHandler> logger) : INotificationHandler<FamilyCreatedEvent>
{
    private readonly ILogger<FamilyCreatedEventHandler> _logger = logger;

    public Task Handle(FamilyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully created.",
            notification.Family.Name, notification.Family.Id);

        return Task.CompletedTask;
    }
}
