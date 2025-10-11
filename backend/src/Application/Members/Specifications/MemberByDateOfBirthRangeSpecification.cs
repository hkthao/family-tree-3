using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications
{
    public class MemberByDateOfBirthRangeSpecification : Specification<Member>
    {
        public MemberByDateOfBirthRangeSpecification(DateTime startDate, DateTime endDate)
        {
            Query.Where(m => m.DateOfBirth >= startDate && m.DateOfBirth <= endDate);
        }
    }
}
