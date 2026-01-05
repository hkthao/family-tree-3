using backend.Domain.Enums;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Commands.CreateLocationLink;

public record CreateLocationLinkCommand : IRequest<Result<Guid>>
{
    public string RefId { get; init; } = null!;
    public RefType RefType { get; init; }
    public string Description { get; init; } = null!;
    public Guid LocationId { get; init; }
}
