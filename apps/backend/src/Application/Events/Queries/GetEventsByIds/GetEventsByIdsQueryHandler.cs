using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser user, IPrivacyService privacyService) : IRequestHandler<GetEventsByIdsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<EventDto>>> Handle(GetEventsByIdsQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return empty list
        if (!_user.IsAuthenticated || _user.UserId == Guid.Empty)
        {
            return Result<List<EventDto>>.Success(new List<EventDto>());
        }

        var eventsQuery = _context.Events.AsQueryable();

        // Apply EventAccessSpecification to filter events based on user's access
        eventsQuery = eventsQuery.WithSpecification(new EventAccessSpecification(_authorizationService.IsAdmin(), _user.UserId));

        var eventsByIdsSpec = new EventsByIdsSpec(request.Ids);
        var eventEntities = await eventsQuery
            .WithSpecification(eventsByIdsSpec)
            .ToListAsync(cancellationToken);

        var eventDtos = _mapper.Map<List<EventDto>>(eventEntities);

        var filteredEventDtos = new List<EventDto>();
        foreach (var eventDto in eventDtos)
        {
            // Assuming eventDto will have FamilyId populated from the mapping.
            // If not, we might need to pass eventEntity.FamilyId explicitly.
            filteredEventDtos.Add(await _privacyService.ApplyPrivacyFilter(eventDto, eventDto.FamilyId ?? Guid.Empty, cancellationToken));
        }

        return Result<List<EventDto>>.Success(filteredEventDtos);
    }
}
