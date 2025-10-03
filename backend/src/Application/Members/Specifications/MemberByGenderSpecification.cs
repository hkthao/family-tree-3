using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByGenderSpecification : BaseSpecification<Member>
{
    public MemberByGenderSpecification(string gender)
    {
        AddCriteria(m => m.Gender == gender);
    }
}
