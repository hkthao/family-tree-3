using Ardalis.Specification;

namespace backend.Application.FamilyMedias.Queries.Specifications;

public class FamilyMediaByIdSpecification : Specification<Domain.Entities.FamilyMedia>, ISingleResultSpecification<Domain.Entities.FamilyMedia>
{
    public FamilyMediaByIdSpecification(Guid id)
    {
        Query
            .Where(fm => fm.Id == id && !fm.IsDeleted)
            .Include(fm => fm.MediaLinks);
    }
}
