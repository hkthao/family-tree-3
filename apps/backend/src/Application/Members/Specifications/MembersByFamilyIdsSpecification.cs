using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MembersByFamilyIdsSpecification : Specification<Member>
{
    public MembersByFamilyIdsSpecification(List<Guid> familyIds)
    {
        Query.Where(m => familyIds.Contains(m.FamilyId));
    }
}
