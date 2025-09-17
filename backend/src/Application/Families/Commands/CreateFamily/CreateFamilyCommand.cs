namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : IRequest<string>
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public string? AvatarUrl { get; init; }
}