using Ardalis.Specification;
using backend.Application.FamilyMedias.Queries.SearchFamilyMedia;

namespace backend.Application.FamilyMedias.Queries.Specifications;

public class SearchFamilyMediaSpecification : Specification<Domain.Entities.FamilyMedia>
{
    public SearchFamilyMediaSpecification(Guid familyId, SearchFamilyMediaQuery? query)
    {
        Query
            .Where(fm => fm.FamilyId == familyId && !fm.IsDeleted)
            .Include(fm => fm.MediaLinks);

        if (query != null)
        {
            if (!string.IsNullOrEmpty(query.SearchQuery))
            {
                var searchQueryLower = query.SearchQuery.ToLower();
                Query.Where(fm =>
                    fm.FileName.ToLower().Contains(searchQueryLower) ||
                    (fm.Description != null && fm.Description.ToLower().Contains(searchQueryLower)));
            }

            if (query.RefId.HasValue)
            {
                Query.Where(fm => fm.MediaLinks.Any(ml => ml.RefId == query.RefId.Value));
            }

            if (query.RefType.HasValue)
            {
                Query.Where(fm => fm.MediaLinks.Any(ml => ml.RefType == query.RefType.Value));
            }

            if (query.MediaType.HasValue)
            {
                Query.Where(fm => fm.MediaType == query.MediaType.Value);
            }
        }
    }
}
