using Ardalis.Specification;
using backend.Application.MemberFaces.Queries.SearchMemberFaces; // Add this using statement
using backend.Domain.Entities;

namespace backend.Application.MemberFaces.Specifications;

public class SearchMemberFacesSpecification : Specification<MemberFace>
{
    public SearchMemberFacesSpecification(SearchMemberFacesQuery query)
    {
        // Include related data
        Query
            .Include(mf => mf.Member)
            .Include(mf => mf.Member!.Family);

        // Filter by FamilyId
        if (query.FamilyId.HasValue)
        {
            Query.Where(mf => mf.Member != null && mf.Member.FamilyId == query.FamilyId.Value);
        }

        // Filter by MemberId
        if (query.MemberId.HasValue)
        {
            Query.Where(mf => mf.MemberId == query.MemberId.Value);
        }

        // Filter by Emotion
        if (!string.IsNullOrWhiteSpace(query.Emotion))
        {
            Query.Where(mf => mf.Emotion != null && mf.Emotion.ToLower() == query.Emotion.ToLower());
        }

        // Filter by SearchQuery (on MemberName or Emotion)
        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            string searchQueryLower = query.SearchQuery.ToLower();
            Query.Where(mf => (mf.Member != null && (mf.Member.FirstName.ToLower().Contains(searchQueryLower) || mf.Member.LastName.ToLower().Contains(searchQueryLower))) || (mf.Emotion != null && mf.Emotion.ToLower().Contains(searchQueryLower)));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            string sortOrder = query.SortOrder?.ToLowerInvariant() ?? "desc"; // Default to descending if not specified

            if (query.SortBy.Equals("membername", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "asc")
                {
                    Query.OrderBy(mf => mf.Member!.LastName).ThenBy(mf => mf.Member.FirstName);
                }
                else
                {
                    Query.OrderByDescending(mf => mf.Member!.LastName).ThenByDescending(mf => mf.Member.FirstName);
                }
            }
            else if (query.SortBy.Equals("familyname", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "asc")
                {
                    Query.OrderBy(mf => mf.Member!.Family!.Name);
                }
                else
                {
                    Query.OrderByDescending(mf => mf.Member!.Family!.Name);
                }
            }
            else
            {
                // Fallback to dynamic expression building for other properties
                // Ardalis.Specification handles simple property names directly, or you can build Expression here if needed
                switch (query.SortBy.ToLowerInvariant())
                {
                    case "faceid":
                        if (sortOrder == "asc") Query.OrderBy(mf => mf.FaceId); else Query.OrderByDescending(mf => mf.FaceId);
                        break;
                    case "confidence":
                        if (sortOrder == "asc") Query.OrderBy(mf => mf.Confidence); else Query.OrderByDescending(mf => mf.Confidence);
                        break;
                    default:
                        if (sortOrder == "asc") Query.OrderBy(mf => mf.Created); else Query.OrderByDescending(mf => mf.Created);
                        break;
                }
            }
        }
        else
        {
            // Default sorting if no SortBy is specified
            Query.OrderByDescending(mf => mf.Created);
        }
    }
}
