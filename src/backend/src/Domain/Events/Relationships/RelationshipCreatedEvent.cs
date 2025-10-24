using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipCreatedEvent : BaseEvent, IDomainEvent
{
    public RelationshipCreatedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
