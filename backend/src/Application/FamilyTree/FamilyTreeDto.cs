using backend.Application.Families;
using backend.Application.Members;

namespace backend.Application.FamilyTree;

public class FamilyTreeDto
{
    public FamilyDto? Family { get; set; }
    public List<MemberDto> Members { get; set; } = new List<MemberDto>();
}
