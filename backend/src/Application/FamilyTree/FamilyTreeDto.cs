using backend.Application.Families;
using backend.Application.Members;
using backend.Domain.Enums;

namespace backend.Application.FamilyTree;

public class FamilyTreeDto
{
    public FamilyDto? Family { get; set; }
    public List<MemberDto> Members { get; set; } = new List<MemberDto>();
    public List<RelationshipDto> Relationships { get; set; } = new List<RelationshipDto>();
}

public class RelationshipDto
{
    public Guid? Id { get; set; }
    public Guid? MemberId { get; set; }
    public RelationshipType Type { get; set; }
    public Guid? SourceMemberId { get; set; }
    public Guid? TargetMemberId { get; set; }
}
