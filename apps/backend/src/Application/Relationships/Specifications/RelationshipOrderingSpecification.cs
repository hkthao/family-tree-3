using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications;

public class RelationshipOrderingSpecification : Specification<Relationship>
{
    public RelationshipOrderingSpecification(string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "sourcememberfullname":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(r => r.SourceMember!.LastName).ThenByDescending(r => r.SourceMember!.FirstName);
                    else
                        Query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName);
                    break;
                case "targetmemberfullname":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(r => r.TargetMember!.LastName).ThenByDescending(r => r.TargetMember!.FirstName);
                    else
                        Query.OrderBy(r => r.TargetMember!.LastName).ThenBy(r => r.TargetMember!.FirstName);
                    break;
                case "sourcemembercode":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(r => r.SourceMember!.Code);
                    else
                        Query.OrderBy(r => r.SourceMember!.Code);
                    break;
                case "targetmembercode":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(r => r.TargetMember!.Code);
                    else
                        Query.OrderBy(r => r.TargetMember!.Code);
                    break;
                case "type":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(r => r.Type);
                    else
                        Query.OrderBy(r => r.Type);
                    break;
                default:
                    Query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName); // Default sort
                    break;
            }
        }
        else
        {
            Query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName); // Default sort if no sortBy is provided
        }
    }
}
