using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Specifications;

public class RelationshipFilterSpecification : Specification<Relationship>
{
    public RelationshipFilterSpecification(Guid? sourceMemberId, Guid? targetMemberId, string? type)
    {
        if (sourceMemberId.HasValue)
        {
            Query.Where(r => r.SourceMemberId == sourceMemberId.Value);
        }

        if (targetMemberId.HasValue)
        {
            Query.Where(r => r.TargetMemberId == targetMemberId.Value);
        }

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<RelationshipType>(type, true, out var relationshipType))
            {
                Query.Where(r => r.Type == relationshipType);
            }
        }
    }
}
