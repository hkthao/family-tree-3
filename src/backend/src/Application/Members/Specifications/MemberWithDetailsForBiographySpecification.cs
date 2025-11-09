using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberWithDetailsForBiographySpecification : Specification<Member>, ISingleResultSpecification<Member>
{
    public MemberWithDetailsForBiographySpecification(Guid memberId)
    {
        Query.Where(m => m.Id == memberId)
             .Include(m => m.Family)
             .Include(m => m.SourceRelationships)
                 .ThenInclude(sr => sr.TargetMember)
             .Include(m => m.TargetRelationships)
                 .ThenInclude(tr => tr.SourceMember);
    }
}
