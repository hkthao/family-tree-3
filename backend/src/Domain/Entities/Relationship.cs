
using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public Guid? FamilyId { get; set; } 
    public Guid? SourceMemberId { get; set; } 
    public Guid? TargetMemberId { get; set; } 
    public RelationshipType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
