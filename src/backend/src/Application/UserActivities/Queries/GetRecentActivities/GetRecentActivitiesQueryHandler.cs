using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Specifications;

namespace backend.Application.UserActivities.Queries.GetRecentActivities;

/// <summary>
/// Handler for fetching recent user activities.
/// </summary>
public class GetRecentActivitiesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user, IAuthorizationService authorizationService) : IRequestHandler<GetRecentActivitiesQuery, Result<List<UserActivityDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<UserActivityDto>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserActivities.AsNoTracking();

        // Apply specifications
        query = query.WithSpecification(new UserActivityByProfileIdSpec(_user.Id!.Value));

        var userActivities = await query
            .ProjectTo<UserActivityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<UserActivityDto>>.Success(userActivities);
    }
}
