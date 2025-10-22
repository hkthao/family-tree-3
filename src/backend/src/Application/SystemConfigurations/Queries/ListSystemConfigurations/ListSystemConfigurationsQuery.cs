using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Queries.ListSystemConfigurations;

public record ListSystemConfigurationsQuery : IRequest<Result<List<SystemConfigurationDto>>>;

public class ListSystemConfigurationsQueryHandler : IRequestHandler<ListSystemConfigurationsQuery, Result<List<SystemConfigurationDto>>>
{
    private readonly IApplicationDbContext _context;

    public ListSystemConfigurationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SystemConfigurationDto>>> Handle(ListSystemConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.SystemConfigurations
            .AsNoTracking()
            .Select(entity => new SystemConfigurationDto
            {
                Id = entity.Id,
                Key = entity.Key,
                Value = entity.Value,
                Description = entity.Description,
                ValueType = entity.ValueType
            })
            .ToListAsync(cancellationToken);

        return Result<List<SystemConfigurationDto>>.Success(entities);
    }
}
