using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyLocationUpdatedEvent : BaseEvent
{
    public FamilyLocationUpdatedEvent(FamilyLocation familyLocation)
    {
        FamilyLocation = familyLocation;
    }

    public FamilyLocation FamilyLocation { get; }
}
