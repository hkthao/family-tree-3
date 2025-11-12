using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Dashboard.Specifications;

public class RelationshipsInFamiliesSpec : Specification<Relationship>
{
    public RelationshipsInFamiliesSpec(IQueryable<Family> familiesQuery)
    {
        Query.Where(r => familiesQuery.Select(f => f.Id).Contains(r.SourceMember.FamilyId));
    }
}
