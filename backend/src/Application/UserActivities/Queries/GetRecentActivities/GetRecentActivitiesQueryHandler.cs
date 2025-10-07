using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Specifications;
using Ardalis.Specification.EntityFrameworkCore;

namespace backend.Application.UserActivities.Queries.GetRecentActivities;

/// <summary>
/// Handler for fetching recent user activities.
/// </summary>
public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, Result<List<UserActivityDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;

    public GetRecentActivitiesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _authorizationService = authorizationService;
    }

    public async Task<Result<List<UserActivityDto>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<List<UserActivityDto>>.Failure("User is not authenticated.", "Authentication");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<List<UserActivityDto>>.Failure("User profile not found.", "NotFound");
        }

        var query = _context.UserActivities.AsNoTracking();

        // Apply specifications
        // query = query.WithSpecification(new UserActivityByProfileIdSpec(currentUserProfile.Id));
        query = query.WithSpecification(new UserActivityByTargetSpec(request.TargetType, request.TargetId));
        query = query.WithSpecification(new UserActivityByGroupSpec(request.GroupId));
        query = query.WithSpecification(new UserActivityOrderingAndPaginationSpec(request.Limit));

        var userActivities = await query
            .ProjectTo<UserActivityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<UserActivityDto>>.Success(userActivities);
    }
}
