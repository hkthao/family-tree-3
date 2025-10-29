using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipUpdatedEvent : BaseEvent, IDomainEvent
{
    public RelationshipUpdatedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
