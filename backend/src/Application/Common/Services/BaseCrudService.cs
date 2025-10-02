using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Common;
using backend.Domain.Common.Interfaces; // Add this
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Services;

public class BaseCrudService<TEntity, TRepository> : IBaseCrudService<TEntity>
    where TEntity : BaseAuditableEntity
    where TRepository : class, IRepository<TEntity>
{
    protected readonly TRepository _repository;
    protected readonly ILogger _logger;
    protected readonly string _serviceName;

    public BaseCrudService(TRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
        _serviceName = typeof(TEntity).Name + "Service";
    }

    protected async Task<Result<TEntity>> FindEntityOrFailAsync(Guid id, string source)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return Result<TEntity>.Failure($"{typeof(TEntity).Name} with ID {id} not found.", source: source);
        }
        return Result<TEntity>.Success(entity);
    }

    public async Task<Result<List<TEntity>>> GetAllAsync()
    {
        var source = $"{_serviceName}.GetAllAsync";
        try
        {
            var entities = await _repository.GetAllAsync();
            return Result<List<TEntity>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source}", source);
            return Result<List<TEntity>>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<TEntity>> GetByIdAsync(Guid id)
    {
        var source = $"{_serviceName}.GetByIdAsync";
        try
        {
            var result = await FindEntityOrFailAsync(id, source);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for ID {Id}", source, id);
            return Result<TEntity>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<TEntity>> CreateAsync(TEntity entity)
    {
        var source = $"{_serviceName}.CreateAsync";
        try
        {
            var createdEntity = await _repository.AddAsync(entity);
            return Result<TEntity>.Success(createdEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for entity {EntityId}", source, entity.Id);
            return Result<TEntity>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result> UpdateAsync(TEntity entity)
    {
        var source = $"{_serviceName}.UpdateAsync";
        try
        {
            var result = await FindEntityOrFailAsync(entity.Id, source);
            if (!result.IsSuccess)
            {
                return Result.Failure(result.Error!, source: result.Source);
            }
            await _repository.UpdateAsync(entity);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for entity {EntityId}", source, entity.Id);
            return Result.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var source = $"{_serviceName}.DeleteAsync";
        try
        {
            var result = await FindEntityOrFailAsync(id, source);
            if (!result.IsSuccess)
            {
                return Result.Failure(result.Error!, source: result.Source);
            }
            await _repository.DeleteAsync(result.Value!);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for ID {Id}", source, id);
            return Result.Failure(ex.Message, source: source);
        }
    }
}