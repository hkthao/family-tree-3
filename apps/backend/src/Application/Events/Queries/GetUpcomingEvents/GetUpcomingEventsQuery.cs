using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetUpcomingEvents;

public record GetUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
{
    public Guid FamilyId { get; init; }
}
