using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Specifications;

public class RelationshipAccessSpecification : Specification<Relationship>
{
    public RelationshipAccessSpecification(bool isAdmin, Guid? currentUserId)
    {
        // Include Family and FamilyUsers to allow filtering based on family access
        Query.Include(r => r.Family!).ThenInclude(f => f.FamilyUsers);

        if (isAdmin)
        {
            // Admin can see all relationships. No additional Where clause is needed.
        }
        else if (currentUserId.HasValue && currentUserId != Guid.Empty)
        {
            // For authenticated non-admin users, show relationships belonging to families
            // they created OR are associated with as a FamilyUser, OR if the family is public.
            Query.Where(r => r.Family != null &&
                            (r.Family.CreatedBy == currentUserId.Value.ToString() ||
                             r.Family.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value) ||
                             r.Family.Visibility == FamilyVisibility.Public.ToString()));
        }
        else // Not admin and not authenticated
        {
            // Unauthenticated users should only see relationships belonging to public families.
            Query.Where(r => r.Family != null && r.Family.Visibility == FamilyVisibility.Public.ToString());
        }
    }
}
