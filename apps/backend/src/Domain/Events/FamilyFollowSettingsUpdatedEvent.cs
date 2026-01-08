using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyFollowSettingsUpdatedEvent : BaseEvent
{
    public FamilyFollow FamilyFollow { get; }

    public FamilyFollowSettingsUpdatedEvent(FamilyFollow familyFollow)
    {
        FamilyFollow = familyFollow;
    }
}
