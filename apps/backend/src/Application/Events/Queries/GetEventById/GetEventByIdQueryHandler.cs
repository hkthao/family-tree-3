using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser user) : IRequestHandler<GetEventByIdQuery, Result<EventDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;

    public async Task<Result<EventDetailDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return failure (or unauthorized)
        if (!_user.IsAuthenticated || _user.UserId == Guid.Empty)
        {
            // If unauthenticated, return Unauthorized error message
            return Result<EventDetailDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication); // Corrected error message and source
        }

        var spec = new EventByIdSpecification(request.Id);

        var query = _context.Events.AsQueryable(); // Removed .Include(e => e.Family)

        // Apply EventAccessSpecification to filter events based on user's access
        query = query.WithSpecification(new EventAccessSpecification(_authorizationService.IsAdmin(), _user.UserId));
        query = query.WithSpecification(spec); // Apply the EventByIdSpecification after access control

        var eventDto = await query
            .ProjectTo<EventDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return eventDto == null
            ? Result<EventDetailDto>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound)
            : Result<EventDetailDto>.Success(eventDto);
    }
}