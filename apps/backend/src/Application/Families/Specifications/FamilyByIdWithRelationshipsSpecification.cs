using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdWithRelationshipsSpecification : Specification<Family>, ISingleResultSpecification<Family>
{
    public FamilyByIdWithRelationshipsSpecification(Guid familyId)
    {
        Query.Where(f => f.Id == familyId)
             .Include(f => f.Relationships);
    }
}
