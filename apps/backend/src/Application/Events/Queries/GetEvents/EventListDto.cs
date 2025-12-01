using backend.Application.Common.Dtos;
using backend.Domain.Enums;

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
    public string? FamilyName { get; set; }
    public string? FamilyAvatarUrl { get; set; }
}
