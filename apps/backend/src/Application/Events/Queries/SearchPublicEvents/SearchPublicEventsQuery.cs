using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.SearchPublicEvents;

public record SearchPublicEventsQuery : PaginatedQuery, IRequest<Result<PaginatedList<EventDto>>>
{
    public string? SearchTerm { get; init; }
    public Guid? FamilyId { get; init; }
    public EventType? EventType { get; init; }
}
