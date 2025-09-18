using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler : IRequestHandler<DeleteRelationshipCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Relationship), request.Id);
        }

        _context.Relationships.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
