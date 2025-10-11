using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using backend.Application.UserProfiles.Specifications; // Added for IRequestHandler

namespace backend.Application.Identity.UserProfiles.Commands.SyncUserProfile
{
    public class SyncUserProfileCommandHandler : IRequestHandler<SyncUserProfileCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SyncUserProfileCommandHandler> _logger;
        private readonly IExternalClaimProvider _externalClaimProvider;

        public SyncUserProfileCommandHandler(
            IApplicationDbContext context,
            ILogger<SyncUserProfileCommandHandler> logger,
            IExternalClaimProvider externalClaimProvider)
        {
            _context = context;
            _logger = logger;
            _externalClaimProvider = externalClaimProvider;
        }

        public async Task<Result<bool>> Handle(SyncUserProfileCommand request, CancellationToken cancellationToken)
        {
            var externalId = _externalClaimProvider.GetExternalId(request.UserPrincipal);
            var email = _externalClaimProvider.GetEmail(request.UserPrincipal);
            var name = _externalClaimProvider.GetName(request.UserPrincipal);

            if (string.IsNullOrEmpty(externalId))
            {
                _logger.LogWarning("External User ID not found in claims. Cannot sync user profile.");
                return Result<bool>.Failure("External User ID not found in claims.", "Authentication");
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
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new user profile for external user {ExternalId}.", externalId);
                    newUserCreated = true;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "A database update exception occurred while creating a user profile. This might be due to a race condition. Attempting to re-fetch the user.");
                    // Attempt to re-fetch the user to handle potential race conditions where the user was created by another request.
                    userProfile = await GetUserProfileByExternalId(externalId, cancellationToken);
                    if (userProfile == null)
                    {
                        _logger.LogError(ex, "Failed to retrieve existing user profile after a DbUpdateException for {ExternalId}. This should not happen.", externalId);
                        throw; // Re-throw if the user still doesn't exist, as it was not a duplicate entry issue.
                    }
                }
            }

            return Result<bool>.Success(newUserCreated);
        }

        private async Task<UserProfile?> GetUserProfileByExternalId(string externalId, CancellationToken cancellationToken)
        {
            return await _context.UserProfiles.WithSpecification(new UserProfileByExternalIdSpecification(externalId)).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
