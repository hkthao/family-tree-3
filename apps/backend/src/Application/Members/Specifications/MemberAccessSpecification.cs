using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Members.Specifications;

public class MemberAccessSpecification : Specification<Member>
{
    public MemberAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Family and FamilyUsers to allow filtering based on family access
        Query.Include(m => m.Family)
             .ThenInclude(f => f.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all members. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show members belonging to families
            // they created OR are associated with as a FamilyUser with Manager or Viewer role, OR if the family is public.
            Query.Where(m => m.Family != null &&
                              (m.Family.CreatedBy == currentUserId.Value.ToString() ||
                               m.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer)) ||
                               m.Family.Visibility == FamilyVisibility.Public.ToString()));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should only see members belonging to public families.
            Query.Where(m => m.Family != null && m.Family.Visibility == FamilyVisibility.Public.ToString());
        }
    }
}
