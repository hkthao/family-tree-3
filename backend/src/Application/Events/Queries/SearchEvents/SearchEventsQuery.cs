using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.SearchEvents;

public record SearchEventsQuery : PaginatedQuery, IRequest<Result<PaginatedList<EventDto>>>
{
    public string? SearchQuery { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Type { get; init; }
    public Guid? FamilyId { get; init; }
    public Guid? MemberId { get; init; }
}
