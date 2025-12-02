namespace backend.Application.Families.Commands.Inputs;

public record class FamilyInput
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarBase64 { get; set; }
    public string Visibility { get; set; } = "Private";
    public IList<FamilyUserDto> FamilyUsers { get; set; } = [];
}
