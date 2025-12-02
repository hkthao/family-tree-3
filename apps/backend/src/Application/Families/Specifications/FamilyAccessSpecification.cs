using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyAccessSpecification : Specification<Family>
{
    public FamilyAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        Query.Include(f => f.FamilyUsers);

        if (isAdmin)
        {
            // If the user is an admin, they can see all families.
            // No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show families they created OR are associated with as a FamilyUser
            Query.Where(f => f.CreatedBy == currentUserId.Value.ToString() || f.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value));
        }
        else // Not admin and not authenticated (currentUserId is null or Guid.Empty)
        {
            // Unauthenticated users should not see any families.
            Query.Where(f => false);
        }
    }
}
