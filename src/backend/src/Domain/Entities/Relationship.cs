using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public Guid SourceMemberId { get; set; }
    public Member SourceMember { get; set; } = null!;
    public Guid TargetMemberId { get; set; }
    public Member TargetMember { get; set; } = null!;
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
}
