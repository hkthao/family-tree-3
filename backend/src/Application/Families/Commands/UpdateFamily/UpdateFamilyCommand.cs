using MediatR;
using MongoDB.Bson;

namespace backend.Application.Families.Commands.UpdateFamily;

public record UpdateFamilyCommand : IRequest
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Address { get; init; }
    public string? Logo { get; init; }
    public string? History { get; init; }
}
