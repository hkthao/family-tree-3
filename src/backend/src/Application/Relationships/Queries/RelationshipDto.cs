using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries;
public class RelationshipDto
{
    public Guid Id { get; set; }
    public Guid SourceMemberId { get; set; }
    public RelationshipMemberDto? SourceMember { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipMemberDto? TargetMember { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
    public Guid FamilyId { get; set; }
}
