using backend.Application.Common.Dtos;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries;

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    // New date and calendar properties
    public CalendarType CalendarType { get; set; }
    public DateTime? SolarDate { get; set; }
    public LunarDateDto? LunarDate { get; set; }
    public RepeatRule RepeatRule { get; set; }

    public Guid? FamilyId { get; set; }
    public EventType Type { get; set; }
    public string? Color { get; set; }
    public string? FamilyName { get; set; }
    public string? FamilyAvatarUrl { get; set; }
    public List<EventMemberDto> EventMembers { get; set; } = [];
    public List<Guid> EventMemberIds
    {
        get
        {
            return [.. EventMembers.Select(e => e.MemberId)];
        }
    }
}
