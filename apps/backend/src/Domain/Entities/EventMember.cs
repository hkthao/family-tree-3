namespace backend.Domain.Entities;

public class EventMember
{
    public Guid EventId { get; private set; }
    public Event Event { get; private set; } = null!;

    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;

    // Private constructor for EF Core
    private EventMember() { }

    public EventMember(Guid eventId, Guid memberId)
    {
        EventId = eventId;
        MemberId = memberId;
    }
}
