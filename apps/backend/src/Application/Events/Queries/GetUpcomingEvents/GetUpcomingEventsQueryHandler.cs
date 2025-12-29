using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications; // Ensure this is present

namespace backend.Application.Events.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser user, IPrivacyService privacyService) : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<EventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Event> eventsQuery = _context.Events;

        // Handle unauthenticated user first, return empty list
        if (!_user.IsAuthenticated || _user.UserId == Guid.Empty)
        {
            return Result<List<EventDto>>.Success(new List<EventDto>());
        }

        // Apply EventAccessSpecification to filter events based on user's access
        eventsQuery = eventsQuery.WithSpecification(new EventAccessSpecification(_authorizationService.IsAdmin(), _user.UserId));
        eventsQuery = eventsQuery.WithSpecification(new EventsByFamilyIdSpecification(request.FamilyId));

        // TEMP: Add basic date filtering for upcoming events for Solar events until EventOccurrenceService is implemented
        eventsQuery = eventsQuery.Where(e =>
            (e.CalendarType == backend.Domain.Enums.CalendarType.Solar && e.SolarDate.HasValue && e.SolarDate.Value > DateTime.UtcNow) ||
            e.CalendarType == backend.Domain.Enums.CalendarType.Lunar // For lunar, we don't have a simple "upcoming" filter here yet
        );

        var upcomingEventsEntities = await eventsQuery
            // Default ordering until EventOccurrenceService is implemented to provide a derived date for sorting
            .OrderBy(e => e.Name) // Placeholder: Order by name for now
            .ToListAsync(cancellationToken);

        var upcomingEventsDtos = _mapper.Map<List<EventDto>>(upcomingEventsEntities);

        var filteredEventDtos = new List<EventDto>();
        foreach (var eventDto in upcomingEventsDtos)
        {
            filteredEventDtos.Add(await _privacyService.ApplyPrivacyFilter(eventDto, request.FamilyId, cancellationToken));
        }

        return Result<List<EventDto>>.Success(filteredEventDtos);
    }
}
