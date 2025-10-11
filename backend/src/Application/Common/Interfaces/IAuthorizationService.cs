using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if the current user is an Admin.
        /// </summary>
        /// <returns>True if the user is an Admin, otherwise false.</returns>
        bool IsAdmin();

        /// <summary>
        /// Retrieves the current user's UserProfile, including their family associations.
        /// </summary>
        /// <returns>The UserProfile if found, otherwise null.</returns>
        Task<UserProfile?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current user has access (Manager or Viewer) to a specific family.
        /// </summary>
        /// <param name="familyId">The ID of the family to check access for.</param>
        /// <param name="userProfile">The current user's profile.</param>
        /// <returns>True if the user has access, otherwise false.</returns>
        bool CanAccessFamily(Guid familyId, UserProfile userProfile);

        /// <summary>
        /// Checks if the current user has management rights (Manager role) for a specific family.
        /// </summary>
        /// <param name="familyId">The ID of the family to check management rights for.</param>
        /// <param name="userProfile">The current user's profile.</param>
        /// <returns>True if the user can manage the family, otherwise false.</returns>
        bool CanManageFamily(Guid familyId, UserProfile userProfile);

        /// <summary>
        /// Checks if the current user has a specific role within a family.
        /// </summary>
        /// <param name="familyId">The ID of the family.</param>
        /// <param name="userProfile">The current user's profile.</param>
        /// <param name="requiredRole">The minimum role required.</param>
        /// <returns>True if the user has the required role or higher, otherwise false.</returns>
        bool HasFamilyRole(Guid familyId, UserProfile userProfile, FamilyRole requiredRole);
    }
}
