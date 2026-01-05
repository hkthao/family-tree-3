using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums; // NEW

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
        var entity = await _context.LocationLinks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.NotFound($"LocationLink with ID {request.Id} not found.");
        }

        entity.Update(
            request.RefId,
            request.RefType,
            request.Description,
            request.LocationId,
            request.LinkType // NEW: Pass LinkType from request
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
