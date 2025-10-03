using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByIdSpecification : BaseSpecification<Member>
{
    public MemberByIdSpecification(Guid id)
    {
        AddCriteria(member => member.Id == id);
    }
}