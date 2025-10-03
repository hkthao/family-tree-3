using backend.Domain.Enums;

namespace backend.Application.Events.Queries.GetEvents;

public class GetEventsQuery : IRequest<IReadOnlyList<EventListDto>>
{
    public string? SearchTerm { get; init; }
    public EventType? EventType { get; init; }
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Location { get; init; }
    public Guid? RelatedMemberId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}