using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;

public class UpdateSystemConfigurationCommandHandler : IRequestHandler<UpdateSystemConfigurationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateSystemConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SystemConfigurations
            .FirstOrDefaultAsync(sc => sc.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result.Failure($"SystemConfiguration with Id {request.Id} not found.");
        }

        entity.Key = request.Key;
        entity.Value = request.Value;
        entity.ValueType = request.ValueType;
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
