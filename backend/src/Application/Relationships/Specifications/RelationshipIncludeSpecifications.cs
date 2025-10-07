using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipIncludeSpecifications: Specification<Relationship>
{
    public RelationshipIncludeSpecifications()
    {
          Query
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember);
    }
}
