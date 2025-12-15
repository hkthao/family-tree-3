using backend.Domain.Enums;
using backend.Application.Events.Commands.Inputs; // Add this

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public EventType Type { get; set; }

    public CalendarType CalendarType { get; set; }
    public DateTime? SolarDate { get; set; }
    public LunarDateInput? LunarDate { get; set; } // Use LunarDateInput for consistency
    public RepeatRule RepeatRule { get; set; }

    public string? Description { get; set; }
    public Guid? FamilyId { get; set; }
    public string? Color { get; set; }
    public List<string> RelatedMembers { get; set; } = [];
}
