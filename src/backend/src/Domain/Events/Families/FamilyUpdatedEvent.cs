using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Families;

public class FamilyUpdatedEvent : BaseEvent, IDomainEvent
{
    public FamilyUpdatedEvent(Family family)
    {
        Family = family;
    }

    public Family Family { get; }
}
