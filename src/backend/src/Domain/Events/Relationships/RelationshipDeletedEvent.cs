using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipDeletedEvent : BaseEvent, IDomainEvent
{
    public RelationshipDeletedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
