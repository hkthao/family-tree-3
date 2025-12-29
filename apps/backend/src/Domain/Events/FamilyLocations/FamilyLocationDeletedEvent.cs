using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyLocationDeletedEvent : BaseEvent
{
    public FamilyLocationDeletedEvent(FamilyLocation familyLocation)
    {
        FamilyLocation = familyLocation;
    }

    public FamilyLocation FamilyLocation { get; }
}
