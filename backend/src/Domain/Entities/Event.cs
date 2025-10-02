using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Event : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public Guid? FamilyId { get; set; }
    public EventType Type { get; set; }
    public string? Color { get; set; }

    public ICollection<Member> RelatedMembers { get; set; } = new List<Member>();
}
