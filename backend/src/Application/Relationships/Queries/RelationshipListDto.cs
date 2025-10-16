using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries;

public class RelationshipListDto
{
    public Guid Id { get; set; }
    public Guid SourceMemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }

    public RelationshipMemberDto? SourceMember { get; set; }
    public RelationshipMemberDto? TargetMember { get; set; }
}
