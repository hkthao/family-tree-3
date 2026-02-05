namespace backend.Application.Families.Commands.GenerateFamilyTreeGraph;

public class GraphGenerationJobDto
{
    public string JobId { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid RootMemberId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
