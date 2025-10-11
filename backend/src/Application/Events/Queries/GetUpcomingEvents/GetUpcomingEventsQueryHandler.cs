using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetUpcomingEvents
{
    public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public GetUpcomingEventsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            _context = context;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<EventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
        {
            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
            if (currentUserProfile == null)
            {
                return Result<List<EventDto>>.Failure("User profile not found.", "NotFound");
            }

            IQueryable<backend.Domain.Entities.Event> eventsQuery = _context.Events;

            if (!_authorizationService.IsAdmin())
            {
                // Filter events by user access if not admin
                var accessibleFamilyIds = await _context.FamilyUsers
                    .Where(fu => fu.UserProfileId == currentUserProfile.Id)
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
}
