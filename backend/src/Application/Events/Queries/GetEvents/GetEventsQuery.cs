using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.GetEvents;

public record class GetEventsQuery : PaginatedQuery, IRequest<IReadOnlyList<EventListDto>>
{
    public string? SearchTerm { get; init; }
    public EventType? EventType { get; init; }
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Location { get; init; }
    public Guid? RelatedMemberId { get; init; }
}