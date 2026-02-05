using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Specifications;

namespace backend.Application.UserActivities.Queries.GetRecentActivities;

/// <summary>
/// Handler for fetching recent user activities.
/// </summary>
public class GetRecentActivitiesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser user) : IRequestHandler<GetRecentActivitiesQuery, Result<PaginatedList<UserActivityDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _user = user;

    public async Task<Result<PaginatedList<UserActivityDto>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserActivities.AsNoTracking();

        // Apply specifications
        query = query.WithSpecification(new UserActivityByUserIdSpec(_user.UserId));

        if (request.GroupId.HasValue)
        {
            query = query.WithSpecification(new UserActivityByGroupIdSpec(request.GroupId.Value));
        }

        var userActivities = await query
            .OrderByDescending(ua => ua.Created) // Apply ordering here
            .ProjectTo<UserActivityDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<UserActivityDto>>.Success(userActivities);
    }
}
