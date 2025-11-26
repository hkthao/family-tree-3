using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class MembersByIdsSpec : Specification<Member>
{
    public MembersByIdsSpec(IEnumerable<Guid> memberIds)
    {
        Query.Where(m => memberIds.Contains(m.Id));
    }
}
