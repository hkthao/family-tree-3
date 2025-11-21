using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyDicts.Commands.DeleteFamilyDict;

public class DeleteFamilyDictCommandHandler : IRequestHandler<DeleteFamilyDictCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFamilyDictCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFamilyDictCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FamilyDicts
            .Where(f => f.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(FamilyDict), request.Id.ToString());
        }

        _context.FamilyDicts.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
