using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Commands.DeleteLocationLink;

public class DeleteLocationLinkCommandHandler : IRequestHandler<DeleteLocationLinkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteLocationLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteLocationLinkCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LocationLinks
            .Where(l => l.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"LocationLink with ID {request.Id} not found.");
        }

        _context.LocationLinks.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
