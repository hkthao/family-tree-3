using backend.Application.Families;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Families.Commands.CreateFamily;

public record CreateFamilyCommand : IRequest<string>
{
    public string? Name { get; init; }
    public string? Address { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
}
