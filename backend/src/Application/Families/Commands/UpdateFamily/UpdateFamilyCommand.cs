namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
}