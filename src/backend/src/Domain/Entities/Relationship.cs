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
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    // Private constructor for EF Core
    private Relationship() { }

    public Relationship(Guid familyId, Guid sourceMemberId, Guid targetMemberId, RelationshipType type)
    {
        FamilyId = familyId;
        SourceMemberId = sourceMemberId;
        TargetMemberId = targetMemberId;
        Type = type;
    }

    public Relationship(Guid familyId)
    {
        FamilyId = familyId;
    }
}
