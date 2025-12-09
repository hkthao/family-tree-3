using backend.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace backend.Application.Services;

public interface IRelationshipDetectionService
{
    Task<RelationshipDetectionResult> DetectRelationshipAsync(Guid familyId, Guid memberAId, Guid memberBId);
}

public class RelationshipDetectionResult
{
    public string FromAToB { get; set; } = "unknown";
    public string FromBToA { get; set; } = "unknown";
    public List<Guid> Path { get; set; } = new List<Guid>();
    public List<string> Edges { get; set; } = new List<string>();
}
