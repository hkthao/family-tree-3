using System.Security.Claims;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandHandler(
    IApplicationDbContext context,
    ILogger<SyncUserProfileCommandHandler> logger) : IRequestHandler<SyncUserProfileCommand, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<SyncUserProfileCommandHandler> _logger = logger;

    public async Task<Result<UserProfileDto>> Handle(SyncUserProfileCommand request, CancellationToken cancellationToken)
    {
        var externalId = request.UserPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = request.UserPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var name = request.UserPrincipal.FindFirst(ClaimTypes.Name)?.Value;
        var firstName = request.UserPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = request.UserPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
        var phone = request.UserPrincipal.FindFirst(ClaimTypes.MobilePhone)?.Value;

        UserProfile? userProfile = null;
        if (!string.IsNullOrEmpty(externalId))
            userProfile = await GetUserProfileByExternalId(externalId, cancellationToken);

        if (userProfile == null)
        {
            // Create new UserProfile
            userProfile = new UserProfile
            {
                ExternalId = externalId ?? "",
                Email = email ?? "",
                Name = name ?? "",
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                Phone = phone ?? "",
                Avatar = ""
            };
            try
            {
                _context.UserProfiles.Add(userProfile);
                // Create default user preferences
                var userPreference = new UserPreference
                {
                    UserProfile = userProfile,
                    Theme = Theme.Dark, // Default theme
                    Language = Language.Vietnamese // Default language
                };
                _context.UserPreferences.Add(userPreference);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created new user profile for external user {ExternalId}.", externalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a user profile for {ExternalId}.", externalId);
                return Result<UserProfileDto>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception); // Return failure instead of re-throwing
            }
        }
        else
        {
            // Update existing UserProfile with default values if properties are empty
            bool changed = false;
            if (string.IsNullOrEmpty(userProfile.FirstName)) { userProfile.FirstName = firstName ?? ""; changed = true; }
            if (string.IsNullOrEmpty(userProfile.LastName)) { userProfile.LastName = lastName ?? ""; changed = true; }
            if (string.IsNullOrEmpty(userProfile.Phone)) { userProfile.Phone = phone ?? ""; changed = true; }
            if (string.IsNullOrEmpty(userProfile.Name)) { userProfile.Name = name ?? ""; changed = true; }
            if (string.IsNullOrEmpty(userProfile.Email)) { userProfile.Email = email ?? ""; changed = true; }
            if (changed)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Updated existing user profile {ExternalId} with default values for missing properties.", externalId);
            }
        }

        // If user already exists, check for profile updates and potentially re-sync with Novu if profile details changed
        // For simplicity, we'll only send welcome notification on first creation for now.
        // More refined logic can be added here to update Novu subscriber details if userProfile fields change.

        return Result<UserProfileDto>.Success(new UserProfileDto()
        {
            Id = userProfile!.Id,
            ExternalId = userProfile.ExternalId,
            Email = userProfile.Email,
            Name = userProfile.Name,
            Phone = userProfile.Phone,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            Avatar = userProfile.Avatar,
        });
    }

    private async Task<UserProfile?> GetUserProfileByExternalId(string externalId, CancellationToken cancellationToken)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == externalId, cancellationToken);
    }
}
