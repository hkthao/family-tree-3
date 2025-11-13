using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipByIdSpec : SingleResultSpecification<Relationship>
{
    public RelationshipByIdSpec(Guid relationshipId)
    {
        Query.Where(r => r.Id == relationshipId);
    }
}
