using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.SearchEvents;

public record SearchEventsQuery : PaginatedQuery, IRequest<PaginatedList<EventDto>>
{
    public string? SearchQuery { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Type { get; init; }
}