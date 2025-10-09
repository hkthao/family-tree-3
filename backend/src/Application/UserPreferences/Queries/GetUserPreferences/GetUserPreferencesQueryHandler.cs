
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Domain.Enums;
using Ardalis.Specification.EntityFrameworkCore;

namespace backend.Application.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, Result<UserPreferenceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetUserPreferencesQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }

    public async Task<Result<UserPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<UserPreferenceDto>.Failure("User is not authenticated.", "Authentication");
        }

        var userProfile = await _context.UserProfiles
            .Include(up => up.UserPreference)
            .WithSpecification(new UserProfileByAuth0IdSpec(currentUserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            return Result<UserPreferenceDto>.Failure("User profile not found.", "NotFound");
        }

        if (userProfile.UserPreference == null)
        {
            // Return default preferences if none exist
            return Result<UserPreferenceDto>.Success(new UserPreferenceDto
            {
                Theme = Theme.Light,
                Language = Language.English,
                EmailNotificationsEnabled = true,
                SmsNotificationsEnabled = false,
                InAppNotificationsEnabled = true,
            });
        }

        return Result<UserPreferenceDto>.Success(_mapper.Map<UserPreferenceDto>(userProfile.UserPreference));
    }
}
