using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;

public class DeleteSystemConfigurationCommandHandler : IRequestHandler<DeleteSystemConfigurationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteSystemConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SystemConfigurations
            .FirstOrDefaultAsync(sc => sc.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result.Failure($"SystemConfiguration with Id {request.Id} not found.");
        }

        _context.SystemConfigurations.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
