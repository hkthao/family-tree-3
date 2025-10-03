using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByNameSpecification : BaseSpecification<Member>
{
    public MemberByNameSpecification(string name)
    {
        AddCriteria(member => member.FirstName.Contains(name) || member.LastName.Contains(name));
    }
}