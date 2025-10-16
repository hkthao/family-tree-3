using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public class AIRelationshipDto 
{
    public string? SourceMemberName { get; set; }
    public Guid? SourceMemberId { get; set; }
    public string? TargetMemberName { get; set; }
    public Guid? TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }

    public List<string> ValidationErrors { get; set; } = new List<string>();
}
