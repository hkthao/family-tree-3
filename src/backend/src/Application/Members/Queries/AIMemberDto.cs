namespace backend.Application.Members.Queries;

public class AIMemberDto : MemberDto
{
    public string? FamilyName { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
