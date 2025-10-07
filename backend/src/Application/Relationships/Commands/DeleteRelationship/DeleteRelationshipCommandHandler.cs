using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler : IRequestHandler<DeleteRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(request.Id);

        if (entity == null)
        {
            return Result<bool>.Failure($"Relationship with ID {request.Id} not found.");
        }

        _context.Relationships.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
