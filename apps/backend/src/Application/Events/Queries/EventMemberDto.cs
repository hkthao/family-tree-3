namespace backend.Application.Events.Queries;

public class EventMemberDto
{
    public Guid EventId { get; set; }
    public Guid MemberId { get; set; }
}
