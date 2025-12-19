
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.Inputs;

public record class EventInput
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }

    public CalendarType CalendarType { get; init; }
    public DateTime? SolarDate { get; init; }
    public LunarDateInput? LunarDate { get; init; } // Use LunarDateInput for command/input
    public RepeatRule RepeatRule { get; init; }

    public Guid? FamilyId { get; init; }
    public EventType Type { get; init; }
    public string? Color { get; init; }
    public ICollection<Guid> RelatedMemberIds { get; init; } = [];
}
