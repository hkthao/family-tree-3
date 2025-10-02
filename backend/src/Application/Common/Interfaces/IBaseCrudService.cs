using backend.Application.Common.Models;
using backend.Domain.Common;

namespace backend.Application.Common.Interfaces;

public interface IBaseCrudService<TEntity, TDto>
    where TEntity : BaseAuditableEntity
    where TDto : class
{
    Task<Result<List<TDto>>> GetAllAsync();
    Task<Result<TDto>> GetByIdAsync(Guid id);
    Task<Result<List<TDto>>> GetByIdsAsync(List<Guid> ids);
    Task<Result<TDto>> CreateAsync(TEntity entity);
    Task<Result> UpdateAsync(TEntity entity);
    Task<Result> DeleteAsync(Guid id);
}