using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdWithMembersAndRelationshipsSpec : SingleResultSpecification<Family>
{
    public FamilyByIdWithMembersAndRelationshipsSpec(Guid familyId)
    {
        Query.Where(f => f.Id == familyId)
             .Include(f => f.Members)
             .Include(f => f.Relationships);
    }
}
