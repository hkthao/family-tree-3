namespace backend.Application.Members.Queries;

public class MemberDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Occupation { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Biography { get; set; }
    public Guid FamilyId { get; set; }
    public bool IsRoot { get; set; }
}
