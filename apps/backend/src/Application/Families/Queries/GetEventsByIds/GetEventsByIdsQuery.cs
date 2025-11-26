using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetEventsByIds;

public record GetEventsByIdsQuery(List<Guid> Ids) : IRequest<Result<List<EventDto>>>;
