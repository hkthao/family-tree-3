using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.GetEventById;

public class EventDetailDto : IMapFrom<Event>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public Guid? FamilyId { get; set; }
    public EventType Type { get; set; }
    public string? Color { get; set; }
    public List<Guid> RelatedMembers { get; set; } = new List<Guid>();
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.RelatedMembers.Select(m => m.Id)));
    }
}