namespace backend.Application.Events.Queries.GetEventById;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDetailDto>;