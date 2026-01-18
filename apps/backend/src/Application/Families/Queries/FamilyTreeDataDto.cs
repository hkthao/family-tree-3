using backend.Application.Members.Queries;
using backend.Application.Relationships.Queries;

namespace backend.Application.Families.Queries;

public class FamilyTreeDataDto
{
    public List<MemberDto> Members { get; set; } = [];
    public List<RelationshipDto> Relationships { get; set; } = [];
    public Guid? RootMemberId { get; set; }
}
