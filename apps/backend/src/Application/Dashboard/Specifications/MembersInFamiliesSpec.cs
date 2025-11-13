using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Dashboard.Specifications;

public class MembersInFamiliesSpec : Specification<Member>
{
    public MembersInFamiliesSpec(IQueryable<Family> familiesQuery)
    {
        Query.Where(m => familiesQuery.Select(f => f.Id).Contains(m.FamilyId));
    }
}
