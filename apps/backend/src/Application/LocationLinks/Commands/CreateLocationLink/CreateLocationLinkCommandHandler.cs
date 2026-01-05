using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums; // NEW

namespace backend.Application.LocationLinks.Commands.CreateLocationLink;

public class CreateLocationLinkCommandHandler : IRequestHandler<CreateLocationLinkCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateLocationLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateLocationLinkCommand request, CancellationToken cancellationToken)
    {
        var entity = LocationLink.Create(
            request.RefId,
            request.RefType,
            request.Description,
            request.LocationId,
            request.LinkType // NEW: Pass LinkType from request
        );

        _context.LocationLinks.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
