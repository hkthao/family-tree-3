using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberPaginationSpecification : Specification<Member>
{
    public MemberPaginationSpecification(int skip, int take)
    {
        Query.Skip(skip).Take(take);
    }
}
