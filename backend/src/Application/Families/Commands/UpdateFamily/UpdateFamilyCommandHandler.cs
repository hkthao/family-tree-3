using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        var entity = await _context.Families.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.AvatarUrl = request.AvatarUrl;

        _context.Families.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}