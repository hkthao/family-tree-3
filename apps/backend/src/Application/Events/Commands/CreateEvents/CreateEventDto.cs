using backend.Domain.Enums;

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public EventType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public Guid? FamilyId { get; set; }
    public List<string> RelatedMembers { get; set; } = [];
}
