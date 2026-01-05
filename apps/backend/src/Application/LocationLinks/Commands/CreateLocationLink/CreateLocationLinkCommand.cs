using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.LocationLinks.Commands.CreateLocationLink;

public record CreateLocationLinkCommand : IRequest<Result<Guid>>
{
    public string RefId { get; init; } = null!;
    public RefType RefType { get; init; }
    public string Description { get; init; } = null!;
    public Guid LocationId { get; init; }
    public LocationLinkType LinkType { get; init; }
}
