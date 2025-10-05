using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByFamilyIdSpecification : Specification<Member>
{
    public MemberByFamilyIdSpecification(Guid? familyId)
    {
        if (familyId.HasValue)
        {
            Query.Where(m => m.FamilyId == familyId.Value);
        }
    }
}
