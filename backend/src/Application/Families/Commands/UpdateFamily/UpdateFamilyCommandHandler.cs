using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Family), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Address = request.Address;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Visibility = request.Visibility;

        // Comment: Write-side invariant: Family is updated in the database context.
        await _context.SaveChangesAsync(cancellationToken);
    }
}