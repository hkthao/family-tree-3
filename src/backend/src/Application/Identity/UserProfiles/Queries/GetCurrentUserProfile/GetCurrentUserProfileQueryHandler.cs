using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper) : IRequestHandler<GetCurrentUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        if (userId == null)
        {
            return Result<UserProfileDto>.Failure("User not authenticated.", "Unauthorized");
        }

        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.ExternalId == userId, cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure("User profile not found.", "NotFound");
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);

        if (_user.Roles != null)
        {
            userProfileDto.Roles = _user.Roles;
        }

        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
