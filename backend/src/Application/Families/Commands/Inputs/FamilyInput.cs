namespace backend.Application.Families.Commands.Inputs;

public  record class FamilyInput
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? AvatarUrl { get; init; }
    public string Visibility { get; init; } = "Private";
}