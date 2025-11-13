namespace backend.Application.Events.Queries;

public class AIEventDto
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? FamilyName { get; set; }
    public Guid? FamilyId { get; set; }
    public List<string> RelatedMembers { get; set; } = new List<string>(); // Can be name or code
    public List<string> ValidationErrors { get; set; } = [];
}
