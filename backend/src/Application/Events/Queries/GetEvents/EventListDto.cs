using backend.Domain.Enums;
using backend.Application.Common.Dtos;

namespace backend.Application.Events.Queries.GetEvents;

public class EventListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public EventType Type { get; set; }
    public Guid? FamilyId { get; set; }
}