
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;
using backend.Domain.Enums;

namespace backend.Application.UserPreferences.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper) : IRequestHandler<GetUserPreferencesQuery, Result<UserPreferenceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .Include(up => up.UserPreference)
            .WithSpecification(new UserProfileByIdSpecification(_user.Id!.Value))
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
