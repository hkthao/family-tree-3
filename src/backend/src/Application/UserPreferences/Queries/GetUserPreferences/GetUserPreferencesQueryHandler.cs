using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IApplicationDbContext context, ICurrentUser user, IMapper mapper) : IRequestHandler<GetUserPreferencesQuery, Result<UserPreferenceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser  _user = user;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .Where(up => up.Id == _user.ProfileId!.Value)
            .Include(up => up.UserPreference)
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            return Result<UserPreferenceDto>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        if (userProfile.UserPreference == null)
        {
            // Return default preferences if none exist
            return Result<UserPreferenceDto>.Success(new UserPreferenceDto
            {
                Theme = Theme.Light,
                Language = Language.English,
            });
        }

        return Result<UserPreferenceDto>.Success(_mapper.Map<UserPreferenceDto>(userProfile.UserPreference));
    }
}
