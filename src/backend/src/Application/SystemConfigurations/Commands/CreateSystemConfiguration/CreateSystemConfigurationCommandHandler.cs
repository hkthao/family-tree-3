using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.SystemConfigurations.Commands.CreateSystemConfiguration;

public class CreateSystemConfigurationCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateSystemConfigurationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<Guid>> Handle(CreateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = new SystemConfiguration
        {
            Key = request.Key,
            Value = request.Value,
            ValueType = request.ValueType,
            Description = request.Description
        };

        _context.SystemConfigurations.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
