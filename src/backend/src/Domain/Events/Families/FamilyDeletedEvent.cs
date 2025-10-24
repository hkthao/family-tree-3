using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Families;

public class FamilyDeletedEvent : BaseEvent
{
    public FamilyDeletedEvent(Family family)
    {
        Family = family;
    }

    public Family Family { get; }
}
