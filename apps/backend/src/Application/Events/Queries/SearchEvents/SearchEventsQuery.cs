using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.SearchEvents;

public record SearchEventsQuery : PaginatedQuery, IRequest<Result<PaginatedList<EventDto>>>
{
    public string? SearchQuery { get; init; }
    public string? Type { get; init; }
    public Guid? FamilyId { get; init; }
    public Guid? MemberId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? LunarStartDay { get; init; }
    public int? LunarStartMonth { get; init; }
    public int? LunarEndDay { get; init; }
    public int? LunarEndMonth { get; init; }
}
