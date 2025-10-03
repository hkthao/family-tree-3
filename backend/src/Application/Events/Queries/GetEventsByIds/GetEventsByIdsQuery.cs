namespace backend.Application.Events.Queries.GetEventsByIds;

public record GetEventsByIdsQuery(List<Guid> Ids) : IRequest<List<EventDto>>;