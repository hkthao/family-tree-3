using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByIdSpecification : Specification<Member>, ISingleResultSpecification<Member>
{
    public MemberByIdSpecification(Guid id)
    {
        Query.Where(m => m.Id == id);
    }
}
