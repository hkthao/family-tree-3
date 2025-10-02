namespace backend.Application.Members.Queries;

public class MemberLookupDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
}
