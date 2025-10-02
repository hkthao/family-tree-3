using backend.Domain.Common;
using backend.Domain.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryRepository<T> : IRepository<T>
    where T : BaseAuditableEntity
{
    protected readonly List<T> _items = new();

    public virtual Task<T?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public virtual Task<IReadOnlyList<T>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<T>>(_items.AsReadOnly());
    }

    public Task<T> AddAsync(T entity)
    {
        entity.Id = Guid.NewGuid(); // Assign a new ID for in-memory entities
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        var existingEntity = _items.FirstOrDefault(x => x.Id == entity.Id);
        if (existingEntity != null)
        {
            _items[_items.IndexOf(existingEntity)] = entity;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _items.RemoveAll(x => x.Id == entity.Id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var result = _items.Where(x => ids.Contains(x.Id)).ToList();
        return Task.FromResult<IReadOnlyList<T>>(result.AsReadOnly());
    }
}