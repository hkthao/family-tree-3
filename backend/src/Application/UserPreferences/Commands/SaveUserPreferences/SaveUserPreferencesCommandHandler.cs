using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences
{
    public class SaveUserPreferencesCommandHandler : IRequestHandler<SaveUserPreferencesCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        private readonly IMapper _mapper;

        public SaveUserPreferencesCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper)
        {
            _context = context;
            _user = user;
            _mapper = mapper;
        }

        public async Task<Result> Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _user.Id;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Result.Failure("User is not authenticated.", "Authentication");
            }

            var userProfile = await _context.UserProfiles
                .Include(up => up.UserPreference)
                .WithSpecification(new UserProfileByAuth0IdSpec(currentUserId))
                .FirstOrDefaultAsync(cancellationToken);

            if (userProfile == null)
            {
                return Result.Failure("User profile not found.", "NotFound");
            }

            if (userProfile.UserPreference == null)
            {
                userProfile.UserPreference = new UserPreference
                {
                    UserProfileId = userProfile.Id,
                };
                _context.UserPreferences.Add(userProfile.UserPreference);
            }

            userProfile.UserPreference.Theme = request.Theme;
            userProfile.UserPreference.Language = request.Language;
            userProfile.UserPreference.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
            userProfile.UserPreference.SmsNotificationsEnabled = request.SmsNotificationsEnabled;
            userProfile.UserPreference.InAppNotificationsEnabled = request.InAppNotificationsEnabled;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
