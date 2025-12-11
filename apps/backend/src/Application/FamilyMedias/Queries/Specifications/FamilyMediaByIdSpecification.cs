using Ardalis.Specification;

namespace backend.Application.FamilyMedias.Queries.Specifications;

public class FamilyMediaByIdSpecification : Specification<backend.Domain.Entities.FamilyMedia>, ISingleResultSpecification<backend.Domain.Entities.FamilyMedia>
{
    public FamilyMediaByIdSpecification(Guid id, Guid familyId)
    {
        Query
            .Where(fm => fm.Id == id && fm.FamilyId == familyId && !fm.IsDeleted)
            .Include(fm => fm.MediaLinks);
    }
}
