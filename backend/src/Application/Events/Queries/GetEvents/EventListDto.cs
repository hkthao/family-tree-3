using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.GetEvents;

public class EventListDto : IMapFrom<Event>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public EventType Type { get; set; }
    public Guid? FamilyId { get; set; }
    public DateTime Created { get; set; }
}