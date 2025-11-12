namespace backend.Domain.Events.Families;

public class FamilyStatsUpdatedEvent : BaseEvent
{
    public FamilyStatsUpdatedEvent(Guid familyId)
    {
        FamilyId = familyId;
    }

    public Guid FamilyId { get; }
}
