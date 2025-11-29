using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class FamilyIdByMemberIdSpecification : Specification<Member, Guid>, ISingleResultSpecification<Member>
{
    public FamilyIdByMemberIdSpecification(Guid memberId)
    {
        Query.Where(m => m.Id == memberId)
             .Select(m => m.FamilyId);
    }
}
