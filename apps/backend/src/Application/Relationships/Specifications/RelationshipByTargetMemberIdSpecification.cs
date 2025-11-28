using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipByTargetMemberIdSpecification : Specification<Relationship>
{
    public RelationshipByTargetMemberIdSpecification(Guid? targetMemberId)
    {
        if (targetMemberId.HasValue)
        {
            Query.Where(r => r.TargetMemberId == targetMemberId.Value);
        }
    }
}
