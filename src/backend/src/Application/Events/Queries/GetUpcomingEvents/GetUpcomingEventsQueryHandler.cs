using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

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

        if (!_authorizationService.IsAdmin())
        {
            // Filter events by user access if not admin
            if (_user.UserId == Guid.Empty)
            {
                return Result<List<EventDto>>.Success([]); // No user ID, no accessible families
            }

            var accessibleFamilyIds = await _context.FamilyUsers
                .Where(fu => fu.UserId == _user.UserId)
                .Select(fu => fu.FamilyId)
                .ToListAsync(cancellationToken);

            eventsQuery = eventsQuery.Where(e => e.FamilyId.HasValue && accessibleFamilyIds.Contains(e.FamilyId.Value));
        }

        if (request.FamilyId.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.FamilyId == request.FamilyId.Value);
        }

        // Apply date range filter
        eventsQuery = eventsQuery.WithSpecification(new EventDateRangeSpecification(request.StartDate, request.EndDate));

        var upcomingEvents = await eventsQuery
            .OrderBy(e => e.StartDate)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(upcomingEvents);
    }
}
