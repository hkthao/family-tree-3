using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Specifications;

public class MemoryItemAccessSpecification : Specification<MemoryItem>
{
    public MemoryItemAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Family and FamilyUsers directly for filtering access
        Query.Include(mi => mi.Family)
             .ThenInclude(f => f!.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all memory items. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show memory items belonging to families
            // they created OR are associated with as a FamilyUser with Manager or Viewer role.
            Query.Where(mi => mi.Family != null &&
                              (mi.Family.CreatedBy == currentUserId.Value.ToString() ||
                               mi.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer))));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should not see any memory items.
            Query.Where(mi => false);
        }
    }
}
