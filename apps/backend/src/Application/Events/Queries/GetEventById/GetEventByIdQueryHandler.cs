using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser user, IPrivacyService privacyService) : IRequestHandler<GetEventByIdQuery, Result<EventDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<EventDetailDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return failure (or unauthorized)
        if (!_user.IsAuthenticated || _user.UserId == Guid.Empty)
        {
            // If unauthenticated, return Unauthorized error message
            return Result<EventDetailDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication); // Corrected error message and source
        }

        var query = _context.Events.Include(e => e.EventMembers).AsQueryable();

        // Apply EventAccessSpecification to filter events based on user's access
        query = query.WithSpecification(new EventAccessSpecification(_authorizationService.IsAdmin(), _user.UserId));
        query = query.WithSpecification(new EventByIdSpecification(request.Id)); // Apply the EventByIdSpecification after access control

        var eventEntity = await query
            .FirstOrDefaultAsync(cancellationToken);

        if (eventEntity == null)
        {
            return Result<EventDetailDto>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound);
        }

        var eventDetailDto = _mapper.Map<EventDetailDto>(eventEntity);

        // Apply privacy filter
        var filteredEventDetailDto = await _privacyService.ApplyPrivacyFilter(eventDetailDto, eventEntity.FamilyId.GetValueOrDefault(), cancellationToken);

        return Result<EventDetailDto>.Success(filteredEventDetailDto);
    }
}
