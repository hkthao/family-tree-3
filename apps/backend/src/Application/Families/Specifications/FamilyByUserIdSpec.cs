using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

/// <summary>
/// Specification to retrieve families that a specific user has access to (either as Manager or Viewer).
/// </summary>
public class FamilyByUserIdSpec : Specification<Family>
{
    public FamilyByUserIdSpec(Guid userId)
    {
        Query
            .Where(f => f.FamilyUsers.Any(fu => fu.UserId == userId));
    }
}
