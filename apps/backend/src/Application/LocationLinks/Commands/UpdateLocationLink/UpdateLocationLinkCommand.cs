using backend.Domain.Enums;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Commands.UpdateLocationLink;

public record UpdateLocationLinkCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string RefId { get; init; } = null!;
    public RefType RefType { get; init; }
    public string Description { get; init; } = null!;
    public Guid FamilyLocationId { get; init; }
}
