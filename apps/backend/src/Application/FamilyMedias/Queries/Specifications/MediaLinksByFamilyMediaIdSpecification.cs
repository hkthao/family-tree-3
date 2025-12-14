using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyMedias.Queries.Specifications;

public class MediaLinksByFamilyMediaIdSpecification : Specification<MediaLink>
{
    public MediaLinksByFamilyMediaIdSpecification(Guid familyMediaId)
    {
        Query
            .Where(ml => ml.FamilyMediaId == familyMediaId)
            .Include(ml => ml.FamilyMedia); // Include FamilyMedia for potential future use or if needed for filtering
    }
}
