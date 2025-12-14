namespace backend.Application.Common.Interfaces;

public interface IRelationshipDetectionService
{
    Task<RelationshipDetectionResult> DetectRelationshipAsync(Guid familyId, Guid memberAId, Guid memberBId, CancellationToken cancellationToken);
}

public class RelationshipDetectionResult
{
    public string Description { get; set; } = "unknown"; // Single descriptive string
    public List<Guid> Path { get; set; } = new List<Guid>();
    public List<string> Edges { get; set; } = new List<string>();
}
