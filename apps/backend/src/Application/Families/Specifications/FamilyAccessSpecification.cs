using Ardalis.Specification;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyAccessSpecification : Specification<Family>
{
    private readonly ICurrentUser _currentUser;

    public FamilyAccessSpecification(Guid? currentUserId, ICurrentUser currentUser)
    {
        _currentUser = currentUser;
        Query.Include(f => f.FamilyUsers);

        if (currentUserId.HasValue && currentUserId != Guid.Empty && _currentUser.IsAuthenticated)
        {
            // For authenticated users, show families they created OR are associated with as a FamilyUser
            Query.Where(f => f.CreatedBy == currentUserId.Value.ToString() || f.FamilyUsers.Any(fu => fu.UserId == currentUserId.Value));
        }
    }
}
