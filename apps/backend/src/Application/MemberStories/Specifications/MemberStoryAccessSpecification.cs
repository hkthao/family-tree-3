using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.MemberStories.Specifications;

public class MemberStoryAccessSpecification : Specification<MemberStory>
{
    public MemberStoryAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Member and FamilyUsers to allow filtering based on family access
        Query.Include(ms => ms.Member)
             .ThenInclude(m => m.Family!)
             .ThenInclude(f => f.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all member stories. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show member stories belonging to families
            // they created OR are associated with as a FamilyUser.
            Query.Where(ms => ms.Member != null && ms.Member.Family != null &&
                              (ms.Member.Family.CreatedBy == currentUserId.Value.ToString() ||
                               ms.Member.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer))));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should not see any member stories.
            Query.Where(ms => false);
        }
    }
}
