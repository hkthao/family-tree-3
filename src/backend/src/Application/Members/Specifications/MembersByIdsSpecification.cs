using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MembersByIdsSpecification : Specification<Member>
{
    public MembersByIdsSpecification(List<Guid> ids)
    {
        Query.Where(m => ids.Contains(m.Id));
        Query.Include(m => m.Relationships);
    }
}
