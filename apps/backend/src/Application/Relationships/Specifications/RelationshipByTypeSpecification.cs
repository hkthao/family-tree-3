using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Specifications;

public class RelationshipByTypeSpecification : Specification<Relationship>
{
    public RelationshipByTypeSpecification(string? type)
    {
        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<RelationshipType>(type, true, out var relationshipType))
            {
                Query.Where(r => r.Type == relationshipType);
            }
        }
    }
}
