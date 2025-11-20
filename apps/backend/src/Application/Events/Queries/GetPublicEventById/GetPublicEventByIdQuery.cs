using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Events.Queries.GetPublicEventById;

public record GetPublicEventByIdQuery(Guid Id) : IRequest<Result<EventDto>>;

public class GetPublicEventByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPublicEventByIdQuery, Result<EventDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<EventDto>> Handle(GetPublicEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == request.Id && e.Family.Visibility == Domain.Enums.FamilyVisibility.Public)
            .ProjectToType<EventDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return eventEntity == null
            ? Result<EventDto>.NotFound($"Event with ID {request.Id} not found or is not public.")
            : Result<EventDto>.Success(eventEntity);
    }
}

public class EventDto : IMapFrom<Event>
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public string EventType { get; set; } = null!;
    public List<Guid> RelatedMembers { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Event, EventDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.RelatedMembers, opt => opt.MapFrom(src => src.RelatedMembers.Select(rm => rm.MemberId).ToList()));
    }
}
