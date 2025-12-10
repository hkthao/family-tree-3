using Ardalis.Specification;
using backend.Application.FamilyMedia.Queries.GetFamilyMediaList;

namespace backend.Application.FamilyMedia.Queries.Specifications;

public class FamilyMediaListSpecification : Specification<Domain.Entities.FamilyMedia>
{
    public FamilyMediaListSpecification(Guid familyId, FamilyMediaFilter? filters)
    {
        Query
            .Where(fm => fm.FamilyId == familyId && !fm.IsDeleted)
            .Include(fm => fm.MediaLinks);

        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.SearchQuery))
            {
                var searchQueryLower = filters.SearchQuery.ToLower();
                Query.Where(fm =>
                    fm.FileName.ToLower().Contains(searchQueryLower) ||
                    (fm.Description != null && fm.Description.ToLower().Contains(searchQueryLower)));
            }

            if (filters.RefId.HasValue)
            {
                Query.Where(fm => fm.MediaLinks.Any(ml => ml.RefId == filters.RefId.Value));
            }

            if (filters.RefType.HasValue)
            {
                Query.Where(fm => fm.MediaLinks.Any(ml => ml.RefType == filters.RefType.Value));
            }

            if (filters.MediaType.HasValue)
            {
                Query.Where(fm => fm.MediaType == filters.MediaType.Value);
            }
        }
    }
}
