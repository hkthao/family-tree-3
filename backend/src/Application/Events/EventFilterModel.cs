using backend.Application.Common.Models;

namespace backend.Application.Events;

public class EventFilterModel : PaginationModel
{
    public string? Name { get; set; }
}
