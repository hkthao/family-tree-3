using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.SystemConfigurations.Queries.SystemConfigurationDto;

namespace FamilyTree.Application.SystemConfigurations.Queries.GetSystemConfiguration;

public record GetSystemConfigurationQuery(string Key) : IRequest<Result<SystemConfigurationDto>>;

public class GetSystemConfigurationQueryHandler : IRequestHandler<GetSystemConfigurationQuery, Result<SystemConfigurationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemConfigurationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SystemConfigurationDto>> Handle(GetSystemConfigurationQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.Key == request.Key, cancellationToken);

        if (entity == null)
        {
            return Result<SystemConfigurationDto>.Failure(new string[] { $"SystemConfiguration with Key {request.Key} not found." });
        }

        var dto = new SystemConfigurationDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Value = entity.Value,
            Description = entity.Description,
            ValueType = entity.ValueType
        };

        return Result<SystemConfigurationDto>.Success(dto);
    }
}
