using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipUpdatedEvent : BaseEvent
{
    public RelationshipUpdatedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
