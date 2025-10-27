using backend.Domain.Entities;

namespace backend.Domain.Events.Families;

public class FamilyCreatedEvent : BaseEvent, IDomainEvent
{
    public FamilyCreatedEvent(Family family)
    {
        Family = family;
    }

    public Family Family { get; }
}
