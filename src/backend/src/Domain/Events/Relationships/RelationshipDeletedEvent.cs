using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Relationships;

public class RelationshipDeletedEvent : BaseEvent
{
    public RelationshipDeletedEvent(Relationship relationship)
    {
        Relationship = relationship;
    }

    public Relationship Relationship { get; }
}
