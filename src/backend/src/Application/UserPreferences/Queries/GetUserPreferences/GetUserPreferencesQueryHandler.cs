using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper) : IRequestHandler<GetUserPreferencesQuery, Result<UserPreferenceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser  _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(up => up.Id == _currentUser.UserId)
            .Include(up => up.Preference)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result<UserPreferenceDto>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        if (user.Preference == null)
        {
            // Return default preferences if none exist
            return Result<UserPreferenceDto>.Success(new UserPreferenceDto
            {
                Theme = Theme.Dark,
                Language = Language.Vietnamese,
            });
        }

        return Result<UserPreferenceDto>.Success(_mapper.Map<UserPreferenceDto>(user.Preference));
    }
}
