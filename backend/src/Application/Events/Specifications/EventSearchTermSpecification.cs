using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications
{
    public class EventSearchTermSpecification : Specification<Event>
    {
        public EventSearchTermSpecification(string? searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                Query.Where(e => e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm)));
            }
        }
    }
}
