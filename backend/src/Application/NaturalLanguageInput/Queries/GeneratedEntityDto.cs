using backend.Application.Families;
using backend.Application.Members.Queries;

namespace backend.Application.NaturalLanguageInput.Queries;

public class GeneratedEntityDto
{
    public string DataType { get; set; } = null!;
    public FamilyDto? Family { get; set; }
    public MemberDto? Member { get; set; }
}
