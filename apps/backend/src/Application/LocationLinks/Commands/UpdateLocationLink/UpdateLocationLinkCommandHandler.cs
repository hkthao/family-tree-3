using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Commands.UpdateLocationLink;

public class UpdateLocationLinkCommandHandler : IRequestHandler<UpdateLocationLinkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateLocationLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateLocationLinkCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LocationLinks
            .Where(l => l.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"LocationLink with ID {request.Id} not found.");
        }

        entity.Update(
            request.RefId,
            request.RefType,
            request.Description,
            request.LocationId
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
