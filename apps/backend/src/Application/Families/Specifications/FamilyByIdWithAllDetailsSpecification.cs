using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdWithAllDetailsSpecification : Specification<Family>, ISingleResultSpecification<Family>
{
    public FamilyByIdWithAllDetailsSpecification(Guid familyId)
    {
        Query.Where(f => f.Id == familyId)
             .Include(f => f.Members)
             .Include(f => f.Relationships)
             .Include(f => f.Events)
                .ThenInclude(e => e.EventMembers); // Include EventMembers for events
    }
}
