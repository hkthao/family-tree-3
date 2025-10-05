using backend.Application.Common.Models; // Added

namespace backend.Application.Events.Queries.GetEventsByIds;

public record GetEventsByIdsQuery(List<Guid> Ids) : IRequest<Result<List<EventDto>>>;