using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetPublicUpcomingEvents;

public record GetPublicUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
{
    public Guid? FamilyId { get; init; }
}

// EventDto is defined in GetPublicEventByIdQuery.cs, so we don't redefine it here.
