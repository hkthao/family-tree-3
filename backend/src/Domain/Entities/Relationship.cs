
using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public string FamilyId { get; set; } = null!;
    public string SourceMemberId { get; set; } = null!;
    public string TargetMemberId { get; set; } = null!;
    public RelationshipType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
