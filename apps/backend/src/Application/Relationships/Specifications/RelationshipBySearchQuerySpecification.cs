using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipBySearchQuerySpecification : Specification<Relationship>
{
    public RelationshipBySearchQuerySpecification(string? searchQuery)
    {
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var lowerSearchQuery = searchQuery.ToLower();
            Query.Where(r =>
                (r.SourceMember != null && (r.SourceMember.FirstName + " " + r.SourceMember.LastName).ToLower().Contains(lowerSearchQuery)) ||
                (r.TargetMember != null && (r.TargetMember.FirstName + " " + r.TargetMember.LastName).ToLower().Contains(lowerSearchQuery)));
        }
    }
}
