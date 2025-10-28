using System.Security.Claims;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandHandler(
    IApplicationDbContext context,
    ILogger<SyncUserProfileCommandHandler> logger,
    INotificationService notificationService) : IRequestHandler<SyncUserProfileCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<SyncUserProfileCommandHandler> _logger = logger;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result<bool>> Handle(SyncUserProfileCommand request, CancellationToken cancellationToken)
    {
        var externalId = request.UserPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = request.UserPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var name = request.UserPrincipal.FindFirst(ClaimTypes.Name)?.Value;

        // Extract first and last name if available from claims
        var firstName = request.UserPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = request.UserPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
        var phone = request.UserPrincipal.FindFirst(ClaimTypes.MobilePhone)?.Value;

        if (string.IsNullOrEmpty(externalId))
        {
            _logger.LogWarning(ErrorMessages.ExternalIdNotFound + ". Cannot sync user profile.");
            return Result<bool>.Failure(ErrorMessages.ExternalIdNotFound, ErrorSources.Authentication);
        }

        var userProfile = await GetUserProfileByExternalId(externalId, cancellationToken);
        bool newUserCreated = false;

        if (userProfile == null)
        {
            // Create new UserProfile
            userProfile = new UserProfile
            {
                ExternalId = externalId,
                Email = email ?? "", // Email might be null if not provided
                Name = name ?? "",   // Name might be null if not provided
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
                newUserCreated = true;

                // Send welcome notification to Novu
                if (userProfile.Email != null)
                {
                    var notificationMessage = new NotificationMessage
                    {
                        RecipientUserId = userProfile.Id.ToString(), // Novu Subscriber ID
                        Title = "Welcome to Family Tree App!",
                        Body = $"Hello {userProfile.Name}, welcome to Family Tree App! We are excited to have you.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "FirstName", firstName ?? userProfile.Name.Split(' ').FirstOrDefault() ?? "" },
                            { "LastName", lastName ?? userProfile.Name.Split(' ').LastOrDefault() ?? "" },
                            { "Email", userProfile.Email },
                            { "Phone", phone ?? "" }
                        }
                    };
                    // The cancellationToken is passed through the entire pipeline
                    await _notificationService.SendNotificationAsync(notificationMessage, cancellationToken);
                    _logger.LogInformation("Sent welcome notification to Novu for new user {ExternalId}.", externalId);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "A database update exception occurred while creating a user profile. This might be due to a race condition. Attempting to re-fetch the user.");
                // Attempt to re-fetch the user to handle potential race conditions where the user was created by another request.
                userProfile = await GetUserProfileByExternalId(externalId, cancellationToken);
                if (userProfile == null)
                {
                    _logger.LogError(ex, "Failed to retrieve existing user profile after a DbUpdateException for {ExternalId}. This should not happen.", externalId);
                    return Result<bool>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Database); // Return failure instead of re-throwing
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a user profile for {ExternalId}.", externalId);
                return Result<bool>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception); // Return failure instead of re-throwing
            }
        }

        // If user already exists, check for profile updates and potentially re-sync with Novu if profile details changed
        // For simplicity, we'll only send welcome notification on first creation for now.
        // More refined logic can be added here to update Novu subscriber details if userProfile fields change.

        return Result<bool>.Success(newUserCreated);
    }

    private async Task<UserProfile?> GetUserProfileByExternalId(string externalId, CancellationToken cancellationToken)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == externalId, cancellationToken);
    }
}
