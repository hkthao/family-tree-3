using backend.Application.Common.Models;
using backend.Domain.Common;

namespace backend.Application.Common.Interfaces;

public interface IBaseCrudService<TEntity>
    where TEntity : BaseAuditableEntity
{
    Task<Result<List<TEntity>>> GetAllAsync();
    Task<Result<TEntity>> GetByIdAsync(Guid id);
    Task<Result<TEntity>> CreateAsync(TEntity entity);
    Task<Result> UpdateAsync(TEntity entity);
    Task<Result> DeleteAsync(Guid id);
}