using System;

namespace backend.Domain.Entities;

public class EventMember
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
}
