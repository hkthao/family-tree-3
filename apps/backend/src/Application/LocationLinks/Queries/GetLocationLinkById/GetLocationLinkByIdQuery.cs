using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Queries.GetLocationLinkById;

public record GetLocationLinkByIdQuery(Guid Id) : IRequest<Result<LocationLinkDto>>;
