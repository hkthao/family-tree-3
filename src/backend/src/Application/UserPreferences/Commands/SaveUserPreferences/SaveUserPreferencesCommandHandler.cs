using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;
using backend.Domain.Entities;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<SaveUserPreferencesCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result> Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .Include(up => up.UserPreference)
            .WithSpecification(new UserProfileByIdSpecification(_user.Id!.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            return Result.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        if (userProfile.UserPreference == null)
        {
            // Check if a UserPreference already exists for this UserProfileId (in case Include failed or was not sufficient)
            var existingUserPreference = await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserProfileId == userProfile.Id, cancellationToken);

            if (existingUserPreference == null)
            {
                userProfile.UserPreference = new UserPreference
                {
                    UserProfileId = userProfile.Id,
                };
                _context.UserPreferences.Add(userProfile.UserPreference);
            }
            else
            {
                // If an existing preference is found, use it instead of creating a new one
                userProfile.UserPreference = existingUserPreference;
            }
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
