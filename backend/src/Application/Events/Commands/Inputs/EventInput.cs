
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.Inputs;

public record class EventInput
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Location { get; init; }
    public Guid? FamilyId { get; init; }
    public EventType Type { get; init; }
    public string? Color { get; init; }
    public ICollection<Guid> RelatedMembers { get; init; } = new List<Guid>();
}
