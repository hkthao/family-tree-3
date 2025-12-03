using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.MemberFaces.Specifications;

public class MemberFaceAccessSpecification : Specification<MemberFace>
{
    public MemberFaceAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Member and Family and FamilyUsers to allow filtering based on family access
        Query.Include(mf => mf.Member)
             .ThenInclude(m => m!.Family)
             .ThenInclude(f => f!.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all member faces. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show member faces belonging to families
            // they created OR are associated with as a FamilyUser with Manager or Viewer role.
            Query.Where(mf => mf.Member != null &&
                              mf.Member.Family != null &&
                              (mf.Member.Family.CreatedBy == currentUserId.Value.ToString() ||
                               mf.Member.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer))));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should not see any member faces.
            Query.Where(mf => false);
        }
    }
}
