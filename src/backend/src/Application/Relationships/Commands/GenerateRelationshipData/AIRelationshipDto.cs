using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public class AIRelationshipDto
{
    public string? SourceMemberName { get; set; }
    public string? SourceMemberCode { get; set; }
    public Guid? SourceMemberId { get; set; }
    public string? TargetMemberName { get; set; }
    public string? TargetMemberCode { get; set; }
    public Guid? TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public string? FamilyName { get; set; }
    public string? FamilyCode { get; set; }
    public Guid? FamilyId { get; set; }

    public List<string> ValidationErrors { get; set; } = [];
}
