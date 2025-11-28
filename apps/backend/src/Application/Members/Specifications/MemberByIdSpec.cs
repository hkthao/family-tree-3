using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByIdSpec : SingleResultSpecification<Member>
{
    public MemberByIdSpec(Guid memberId)
    {
        Query.Where(m => m.Id == memberId);
    }
}
