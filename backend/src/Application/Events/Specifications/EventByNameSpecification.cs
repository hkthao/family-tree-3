using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications
{
    public class EventByNameSpecification : Specification<Event>
    {
        public EventByNameSpecification(string name)
        {
            Query.Where(e => e.Name.Contains(name));
        }
    }
}
