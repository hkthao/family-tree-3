using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventOrderingSpecification : Specification<Event>
{
    public EventOrderingSpecification(string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(e => e.Name);
                    else
                        Query.OrderBy(e => e.Name);
                    break;
                case "code":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(e => e.Code);
                    else
                        Query.OrderBy(e => e.Code);
                    break;
                case "startdate":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(e => e.StartDate);
                    else
                        Query.OrderBy(e => e.StartDate);
                    break;
                case "location": // Added
                    if (sortOrder == "desc")
                        Query.OrderByDescending(e => e.Location);
                    else
                        Query.OrderBy(e => e.Location);
                    break;
                case "created":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(e => e.Created);
                    else
                        Query.OrderBy(e => e.Created);
                    break;
                default:
                    Query.OrderBy(e => e.StartDate); // Default sort
                    break;
            }
        }
        else
        {
            Query.OrderBy(e => e.StartDate); // Default sort if no sortBy is provided
        }
    }
}
