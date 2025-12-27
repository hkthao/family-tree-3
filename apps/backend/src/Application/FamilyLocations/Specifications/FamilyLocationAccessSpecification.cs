using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationAccessSpecification : Specification<FamilyLocation>
{
    public FamilyLocationAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Family and FamilyUsers directly for filtering access
        Query.Include(l => l.Family)
             .ThenInclude(f => f!.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all family locations. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show family locations belonging to families
            // they created OR are associated with as a FamilyUser with Manager or Viewer role, OR if the family is public.
            Query.Where(l => l.Family != null &&
                              (l.Family.CreatedBy == currentUserId.Value.ToString() ||
                               l.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer)) ||
                               l.Family.Visibility == FamilyVisibility.Public.ToString()));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should only see family locations belonging to public families.
            Query.Where(l => l.Family != null && l.Family.Visibility == FamilyVisibility.Public.ToString());
        }
    }
}
