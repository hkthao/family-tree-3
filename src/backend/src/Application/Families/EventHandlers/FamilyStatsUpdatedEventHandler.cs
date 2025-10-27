using backend.Application.Common.Interfaces;
using backend.Domain.Events.Families;
using MediatR;

namespace backend.Application.Families.EventHandlers;

public class FamilyStatsUpdatedEventHandler(IFamilyTreeService familyTreeService) : INotificationHandler<FamilyStatsUpdatedEvent>
{
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task Handle(FamilyStatsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _familyTreeService.UpdateFamilyStats(notification.FamilyId, cancellationToken);
    }
}