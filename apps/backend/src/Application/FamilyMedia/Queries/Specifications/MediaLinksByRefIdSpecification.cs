using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Queries.Specifications;

public class MediaLinksByRefIdSpecification : Specification<MediaLink>
{
    public MediaLinksByRefIdSpecification(Guid refId, RefType refType, Guid familyId)
    {
        Query
            .Where(ml => ml.RefId == refId && ml.RefType == refType && ml.FamilyMedia.FamilyId == familyId && !ml.FamilyMedia.IsDeleted)
            .Include(ml => ml.FamilyMedia);
    }
}
