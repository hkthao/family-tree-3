using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsByDateRangeSpecification : Specification<Event>
{
    public EventsByDateRangeSpecification(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue)
        {
            Query.Where(e => e.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(e => e.StartDate <= endDate.Value);
        }
    }
}
