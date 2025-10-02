namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : IRequest<Guid>
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }
    
    public string? Address { get; init; }

    public string? AvatarUrl { get; init; }
}
