using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByNameSpecification : Specification<Member>
{
    public MemberByNameSpecification(string name)
    {
        Query.Where(m => m.FirstName.Contains(name) || m.LastName.Contains(name));
    }
}
