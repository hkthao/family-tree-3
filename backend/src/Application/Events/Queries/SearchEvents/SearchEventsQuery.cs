using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.SearchEvents;

public record SearchEventsQuery : IRequest<PaginatedList<EventDto>>
{
    public string? SearchQuery { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
