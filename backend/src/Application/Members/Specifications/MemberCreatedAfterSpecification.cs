using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberCreatedAfterSpecification : BaseSpecification<Member>
{
    public MemberCreatedAfterSpecification(DateTime date)
    {
        AddCriteria(member => member.Created > date);
    }
}