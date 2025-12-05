using Ardalis.Specification;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyLinks.Specifications;

public class FamilyLinkByFamilyIdSpecification : Specification<FamilyLink>
{
    public FamilyLinkByFamilyIdSpecification(Guid familyId)
    {
        Query.Where(fl => fl.Family1Id == familyId || fl.Family2Id == familyId)
             .Include(fl => fl.Family1)
             .Include(fl => fl.Family2);
    }
}
