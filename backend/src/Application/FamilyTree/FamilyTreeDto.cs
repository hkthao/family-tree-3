using backend.Application.Families;
using backend.Application.Members;
using backend.Application.Relationships; // Add this using statement
using backend.Domain.Enums;

namespace backend.Application.FamilyTree;

public class FamilyTreeDto
{
    public FamilyDto? Family { get; set; }
    public List<MemberDto> Members { get; set; } = new List<MemberDto>();
    public List<RelationshipDto> Relationships { get; set; } = new List<RelationshipDto>();
}
