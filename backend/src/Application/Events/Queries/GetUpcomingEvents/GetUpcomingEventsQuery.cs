using MediatR;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;

namespace backend.Application.Events.Queries.GetUpcomingEvents;

public record GetUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
{
    public Guid? FamilyId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
