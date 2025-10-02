using backend.Application.Common.Models;
using backend.Application.Events;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IEventService
{
    Task<Result<List<EventDto>>> GetAllAsync();
    Task<Result<EventDto>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(Event anEvent);
    Task<Result<bool>> UpdateAsync(Event anEvent);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<PaginatedList<EventDto>>> SearchAsync(EventFilterModel filter);
}
