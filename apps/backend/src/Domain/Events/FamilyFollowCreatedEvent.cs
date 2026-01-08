using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyFollowCreatedEvent : BaseEvent
{
    public FamilyFollow FamilyFollow { get; }

    public FamilyFollowCreatedEvent(FamilyFollow familyFollow)
    {
        FamilyFollow = familyFollow;
    }
}
