using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

public class FamilyFollowDeletedEvent : BaseEvent
{
    public FamilyFollow FamilyFollow { get; }

    public FamilyFollowDeletedEvent(FamilyFollow familyFollow)
    {
        FamilyFollow = familyFollow;
    }
}
