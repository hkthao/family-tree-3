using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace backend.Application.Services;

public class UserProfileSyncService : IUserProfileSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserProfileSyncService> _logger;

    public UserProfileSyncService(IApplicationDbContext context, ILogger<UserProfileSyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SyncUserProfileAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var auth0UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(auth0UserId))
        {
            _logger.LogWarning("Auth0 User ID not found in claims. Cannot sync user profile.");
            return;
        }

        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.Auth0UserId == auth0UserId, cancellationToken);

        if (userProfile == null)
        {
            // Create new UserProfile
            userProfile = new UserProfile
            {
                Auth0UserId = auth0UserId,
                Email = email ?? "", // Email might be null if not provided by Auth0
                Name = name ?? "",   // Name might be null if not provided by Auth0
            };
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created new user profile for Auth0 user {Auth0UserId}.", auth0UserId);
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
