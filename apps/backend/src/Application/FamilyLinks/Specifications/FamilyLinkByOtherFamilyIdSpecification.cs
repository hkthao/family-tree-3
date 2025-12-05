using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLinks.Specifications;

public class FamilyLinkByOtherFamilyIdSpecification : Specification<FamilyLink>
{
    public FamilyLinkByOtherFamilyIdSpecification(Guid otherFamilyId)
    {
        Query.Where(fl => fl.Family1Id == otherFamilyId || fl.Family2Id == otherFamilyId);
    }
}
