using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.AI.Specifications
{
    public class LastAIBiographyByMemberIdSpec : Specification<AIBiography>, ISingleResultSpecification<AIBiography>
    {
        public LastAIBiographyByMemberIdSpec(Guid memberId)
        {
            Query
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.Created);
        }
    }
}
