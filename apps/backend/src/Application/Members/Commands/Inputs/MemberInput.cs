namespace backend.Application.Members.Inputs;

public record MemberInput
{
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public Guid? Id { get; set; }
    public string? Code { get; set; }
    public string? Nickname { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Occupation { get; set; }
    public string? Biography { get; set; }
    public Guid FamilyId { get; set; }
    public bool IsRoot { get; set; }
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? HusbandId { get; set; }
    public Guid? WifeId { get; set; }
}
