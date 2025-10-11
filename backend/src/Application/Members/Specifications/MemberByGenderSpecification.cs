using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications
{
    public class MemberByGenderSpecification : Specification<Member>
    {
        public MemberByGenderSpecification(string? gender)
        {
            if (!string.IsNullOrEmpty(gender))
            {
                Query.Where(m => m.Gender == gender);
            }
        }
    }
}
