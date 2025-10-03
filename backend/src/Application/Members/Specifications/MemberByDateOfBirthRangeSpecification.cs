using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByDateOfBirthRangeSpecification : BaseSpecification<Member>
{
    public MemberByDateOfBirthRangeSpecification(DateTime startDate, DateTime endDate)
    {
        AddCriteria(m => m.DateOfBirth >= startDate && m.DateOfBirth <= endDate);
    }
}
