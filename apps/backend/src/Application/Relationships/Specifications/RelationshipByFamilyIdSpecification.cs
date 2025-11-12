using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipByFamilyIdSpecification : Specification<Relationship>
{
    public RelationshipByFamilyIdSpecification(Guid? familyId)
    {
        if (familyId.HasValue)
        {
            Query.Where(r => r.SourceMember.FamilyId == familyId.Value);
        }
    }
}
