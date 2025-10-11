using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipBySourceMemberIdSpecification : Specification<Relationship>
{
    public RelationshipBySourceMemberIdSpecification(Guid? sourceMemberId)
    {
        if (sourceMemberId.HasValue)
        {
            Query.Where(r => r.SourceMemberId == sourceMemberId.Value);
        }
    }
}
