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
    public string? Id { get; set; }
    public string? MemberId { get; set; }
    public RelationshipType Type { get; set; }
    public string? TargetId { get; set; }
}
