using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsBySearchQuerySpecification : Specification<Event>
{
    public EventsBySearchQuerySpecification(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(e => (e.Name != null && e.Name.Contains(searchTerm)) ||
                             (e.Description != null && e.Description.Contains(searchTerm)));
        }
    }
}
