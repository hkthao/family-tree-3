using Ardalis.Specification.EntityFrameworkCore; // Add this
using backend.Application.Events.Specifications; // Add this
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetEventsByMemberId;

public class GetEventsByMemberIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    backend.Application.Common.Interfaces.IAuthorizationService authorizationService,
    ICurrentUser user,
    IPrivacyService privacyService,
    ILogger<GetEventsByMemberIdQueryHandler> logger
) : IRequestHandler<GetEventsByMemberIdQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IPrivacyService _privacyService = privacyService;
    private readonly ILogger<GetEventsByMemberIdQueryHandler> _logger = logger;

    public async Task<Result<List<EventDto>>> Handle(GetEventsByMemberIdQuery request, CancellationToken cancellationToken)
    {
        // Apply EventAccessSpecification to filter events based on user's access
        IQueryable<Event> eventsQuery = _context.Events
            .Include(e => e.EventMembers)
            .Where(e => e.EventMembers.Any(em => em.MemberId == request.MemberId));

        eventsQuery = eventsQuery.WithSpecification(new EventAccessSpecification(_authorizationService.IsAdmin(), _user.UserId));

        var events = await eventsQuery
            .OrderBy(e => e.SolarDate)
            .ToListAsync(cancellationToken);

        if (!events.Any())
        {
            return Result<List<EventDto>>.Success(new List<EventDto>());
        }

        var eventDtos = _mapper.Map<List<EventDto>>(events);

        var filteredEventDtos = new List<EventDto>();
        foreach (var eventDto in eventDtos)
        {
            if (eventDto.FamilyId.HasValue)
            {
                // Apply privacy filter to each EventDto
                filteredEventDtos.Add(await _privacyService.ApplyPrivacyFilter(eventDto, eventDto.FamilyId.Value, cancellationToken));
            }
            else
            {
                // If FamilyId is null, skip privacy filter (or handle as appropriate for your domain logic)
                // For now, we'll just add it without filtering, assuming it's meant to be public or handled elsewhere.
                _logger.LogWarning("Event {EventId} has a null FamilyId. Skipping privacy filter for this event.", eventDto.Id);
                filteredEventDtos.Add(eventDto);
            }
        }

        return Result<List<EventDto>>.Success(filteredEventDtos);
    }
}
