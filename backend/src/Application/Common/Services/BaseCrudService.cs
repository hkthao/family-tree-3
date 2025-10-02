using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Common;
using backend.Domain.Common.Interfaces; // Add this
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Services;

public class BaseCrudService<TEntity, TRepository, TDto> : IBaseCrudService<TEntity, TDto>
    where TEntity : BaseAuditableEntity
    where TDto : class
    where TRepository : class, IRepository<TEntity>
{
    protected readonly TRepository _repository;
    protected readonly ILogger _logger;
    protected readonly IMapper _mapper;
    protected readonly string _serviceName;

    public BaseCrudService(TRepository repository, ILogger logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _serviceName = typeof(TEntity).Name + "Service";
    }

    protected async Task<Result<TDto>> FindEntityOrFailAsync(Guid id, string source)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return Result<TDto>.Failure($"{typeof(TEntity).Name} with ID {id} not found.", source: source);
        }
        return Result<TDto>.Success(_mapper.Map<TDto>(entity));
    }

    public virtual async Task<Result<List<TDto>>> GetAllAsync()
    {
        var source = $"{_serviceName}.GetAllAsync";
        try
        {
            var entities = await _repository.GetAllAsync();
            return Result<List<TDto>>.Success(_mapper.Map<List<TDto>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source}", source);
            return Result<List<TDto>>.Failure(ex.Message, source: source);
        }
    }

    public virtual async Task<Result<TDto>> GetByIdAsync(Guid id)
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
            return Result<TDto>.Failure(ex.Message, source: source);
        }
    }

     public virtual async Task<Result<List<TDto>>> GetByIdsAsync(List<Guid> ids)
    {
        var source = $"{_serviceName}.GetByIdAsync";
        try
        {
            var result = await _repository.GetByIdsAsync(ids);
            return Result<List<TDto>>.Success(_mapper.Map<List<TDto>>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for IDs {Ids}", source, ids);
            return Result<List<TDto>>.Failure(ex.Message, source: source);
        }
    }

    public virtual async Task<Result<TDto>> CreateAsync(TEntity entity)
    {
        var source = $"{_serviceName}.CreateAsync";
        try
        {
            var createdEntity = await _repository.AddAsync(entity);
            return Result<TDto>.Success(_mapper.Map<TDto>(createdEntity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for entity {EntityId}", source, entity.Id);
            return Result<TDto>.Failure(ex.Message, source: source);
        }
    }

    public virtual async Task<Result> UpdateAsync(TEntity entity)
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

    public virtual async Task<Result> DeleteAsync(Guid id)
    {
        var source = $"{_serviceName}.DeleteAsync";
        try
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return Result.Failure($"{typeof(TEntity).Name} with ID {id} not found.", source: source);
            await _repository.DeleteAsync(result);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for ID {Id}", source, id);
            return Result.Failure(ex.Message, source: source);
        }
    }
}