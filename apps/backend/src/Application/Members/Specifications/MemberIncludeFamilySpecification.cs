using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberIncludeFamilySpecification : Specification<Member>
{
    public MemberIncludeFamilySpecification()
    {
        Query.Include(m => m.Family);
    }
}
