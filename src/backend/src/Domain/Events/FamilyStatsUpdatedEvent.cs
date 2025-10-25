using backend.Domain.Common;

namespace backend.Domain.Events;

public class FamilyStatsUpdatedEvent : BaseEvent
{
    public Guid FamilyId { get; }

    public FamilyStatsUpdatedEvent(Guid familyId)
    {
        FamilyId = familyId;
    }
}
