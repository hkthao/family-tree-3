using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByIsRootSpecification : Specification<Member>, ISingleResultSpecification<Member>
{
    public MemberByIsRootSpecification(Guid excludeMemberId)
    {
        Query.Where(m => m.IsRoot && m.Id != excludeMemberId);
    }

    public MemberByIsRootSpecification()
    {
        Query.Where(m => m.IsRoot);
    }
}
