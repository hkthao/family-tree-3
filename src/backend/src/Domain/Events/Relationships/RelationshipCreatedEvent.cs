using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipCreatedEvent : BaseEvent
{
    public RelationshipCreatedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
