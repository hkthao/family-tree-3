using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdWithMembersAndRelationshipsSpecification : Specification<Family>, ISingleResultSpecification<Family>
{
    public FamilyByIdWithMembersAndRelationshipsSpecification(Guid familyId)
    {
        Query.Where(f => f.Id == familyId)
             .Include(f => f.Members)
             .Include(f => f.Relationships);
    }
}
