
namespace backend.Application.Common.Interfaces;

using backend.Domain.Entities;

/// <summary>
/// Defines the contract for a service that manages subscribers in Novu.
/// </summary>
public interface INovuSubscriberService
{
    /// <summary>
    /// Creates or updates a subscriber in Novu based on the application user profile.
    /// </summary>
    /// <param name="userProfile">The user profile to sync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SyncUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken);
}
