using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;

namespace backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(IApplicationDbContext context, ICurrentUser user, IMapper mapper) : IRequestHandler<GetCurrentUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _user = user;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userProfileSpec = new UserProfileByUserIdSpec(_user.UserId);
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .WithSpecification(userProfileSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);

        if (_user.Roles != null)
        {
            userProfileDto.Roles = _user.Roles;
        }

        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
