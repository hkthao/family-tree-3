using System.Security.Claims;

namespace backend.Application.Common.Interfaces;

public interface IUserProfileSyncService
{
    Task<bool> SyncUserProfileAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<backend.Domain.Entities.UserProfile?> GetUserProfileByAuth0Id(string auth0UserId);
}
