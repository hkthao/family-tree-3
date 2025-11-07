using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Specifications;
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

            var familyUsersSpec = new FamilyUsersByUserIdSpec(_user.UserId);
            var accessibleFamilyIds = await _context.FamilyUsers
                .WithSpecification(familyUsersSpec)
                .Select(fu => fu.FamilyId)
                .ToListAsync(cancellationToken);

            // If no accessible families, return empty list
            if (!accessibleFamilyIds.Any())
            {
                return Result<List<EventDto>>.Success([]);
            }

            eventsQuery = eventsQuery.WithSpecification(new EventsByFamilyIdsSpec(accessibleFamilyIds));
        }

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
