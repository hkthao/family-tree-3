namespace backend.Domain.Common.Interfaces;

public interface IRepository<T>
    where T : BaseAuditableEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<Guid> ids);
}