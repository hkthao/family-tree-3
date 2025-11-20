using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberIncludeRelationshipsSpecification : Specification<Member>
{
    public MemberIncludeRelationshipsSpecification()
    {
        Query.Include(m => m.SourceRelationships)
            .ThenInclude(r => r.TargetMember);
        Query.Include(m => m.TargetRelationships)
            .ThenInclude(r => r.SourceMember);
    }
}
