using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyLocationCreatedEvent : BaseEvent
{
    public FamilyLocationCreatedEvent(FamilyLocation familyLocation)
    {
        FamilyLocation = familyLocation;
    }

    public FamilyLocation FamilyLocation { get; }
}
