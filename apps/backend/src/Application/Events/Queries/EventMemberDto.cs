using backend.Domain.Enums; // Add this

namespace backend.Application.Events.Queries;

public class EventMemberDto 
{
    public Guid EventId { get; set; }
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public Gender Gender { get; set; }
}
