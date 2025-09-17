using MediatR;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : IRequest
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Address { get; init; }
}