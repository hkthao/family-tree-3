using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using backend.Application.UserProfiles.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Application.Services;

public class UserProfileSyncService : IUserProfileSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserProfileSyncService> _logger;
    private readonly IAuth0Config _auth0Config;

    public UserProfileSyncService(IApplicationDbContext context, ILogger<UserProfileSyncService> logger, IAuth0Config auth0Config)
    {
        _context = context;
        _logger = logger;
        _auth0Config = auth0Config;
    }

    public async Task SyncUserProfileAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var auth0UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal.FindFirst($"{_auth0Config.Namespace}email")?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal.FindFirst($"{_auth0Config.Namespace}name")?.Value ?? principal.FindFirst(ClaimTypes.Name)?.Value;

        // TODO: Auth0 ID tokens might not contain 'email' and 'name' claims directly.
        // These claims are typically available in the Access Token or via the /userinfo endpoint.
        // If 'email' and 'name' are consistently null, consider:
        // 1. Configuring Auth0 to include these claims in the ID token.
        // 2. Making a call to the /userinfo endpoint to retrieve full profile data.

        if (string.IsNullOrEmpty(auth0UserId))
        {
            _logger.LogWarning("Auth0 User ID not found in claims. Cannot sync user profile.");
            return;
        }

        var userProfile = await _context.UserProfiles.WithSpecification(new UserProfileByAuth0UserIdSpecification(auth0UserId)).FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            // Create new UserProfile
            userProfile = new UserProfile
            {
                Auth0UserId = auth0UserId,
                Email = email ?? "", // Email might be null if not provided by Auth0
                Name = name ?? "",   // Name might be null if not provided by Auth0
            };
            try
            {
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created new user profile for Auth0 user {Auth0UserId}.", auth0UserId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "A database update exception occurred while creating a user profile. This might be due to a race condition. Attempting to re-fetch the user.");
                // Attempt to re-fetch the user to handle potential race conditions where the user was created by another request.
                userProfile = await _context.UserProfiles.WithSpecification(new UserProfileByAuth0UserIdSpecification(auth0UserId)).FirstOrDefaultAsync(cancellationToken);
                if (userProfile == null)
                {
                    _logger.LogError(ex, "Failed to retrieve existing user profile after a DbUpdateException for {Auth0UserId}. The original exception will be re-thrown.", auth0UserId);
                    throw; // Re-throw if the user still doesn't exist, as it was not a duplicate entry issue.
                }
            }
        }
        else
        {
            // Update existing UserProfile if details have changed
            bool changed = false;
            if (email != null && userProfile.Email != email)
            {
                userProfile.Email = email;
                changed = true;
            }
            if (name != null && userProfile.Name != name)
            {
                userProfile.Name = name;
                changed = true;
            }

            if (changed)
            {
                _logger.LogInformation("Updated existing user profile for Auth0 user {Auth0UserId}.", auth0UserId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
