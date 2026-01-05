using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Commands.DeleteLocationLink;

public record DeleteLocationLinkCommand(Guid Id) : IRequest<Result<bool>>;
