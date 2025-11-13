using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberIncludeRelationshipsSpecification : Specification<Member>
{
    public MemberIncludeRelationshipsSpecification()
    {
        Query.Include(m => m.SourceRelationships);
        Query.Include(m => m.TargetRelationships);
    }
}
