using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetPublicEventById;

public record GetPublicEventByIdQuery(Guid Id) : IRequest<Result<EventDto>>;
