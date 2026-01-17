using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Dashboard.Specifications;

public class FamiliesCountSpec : Specification<Family>
{
    public FamiliesCountSpec(IEnumerable<Guid>? accessibleFamilyIds, Guid familyId)
    {
        if (accessibleFamilyIds != null)
        {
            if (accessibleFamilyIds.Any())
            {
                Query.Where(f => accessibleFamilyIds.Contains(f.Id));
            }
            else
            {
                // If accessibleFamilyIds is empty, no families should be returned
                Query.Where(f => false);
            }
        }

        Query.Where(f => f.Id == familyId);
    }
}
