using backend.Application.Common.Dtos;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.GetEventById;

public class EventDetailDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? FamilyId { get; set; }
    public EventType Type { get; set; }
    public string? Color { get; set; }
    public string? FamilyName { get; set; }
    public string? FamilyAvatarUrl { get; set; }

    // New fields from Event domain entity
    public CalendarType CalendarType { get; set; }
    public DateTime? SolarDate { get; set; }
    public LunarDateDto? LunarDate { get; set; } // Use the new DTO
    public RepeatRule RepeatRule { get; set; }
    public List<EventMemberDto> EventMembers { get; set; } = [];
    public List<Guid> EventMemberIds
    {
        get
        {
            return [.. EventMembers.Select(e => e.MemberId)];
        }
    }
}
