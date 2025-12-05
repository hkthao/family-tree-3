using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications; // Ensure this is present

namespace backend.Application.Events.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser user) : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;

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


        if (request.FamilyId.HasValue)
        {
            eventsQuery = eventsQuery.WithSpecification(new EventByFamilyIdSpecification(request.FamilyId));
        }

        // Apply date range filter
        eventsQuery = eventsQuery.WithSpecification(new EventDateRangeSpecification(request.StartDate, request.EndDate));

        var upcomingEvents = await eventsQuery
            .WithSpecification(new EventOrderByStartDateSpec())
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(upcomingEvents);
    }
}
