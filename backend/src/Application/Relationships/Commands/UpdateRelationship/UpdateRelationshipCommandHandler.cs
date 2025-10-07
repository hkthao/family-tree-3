using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandler : IRequestHandler<UpdateRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(request.Id);

        if (entity == null)
        {
            return Result<bool>.Failure($"Relationship with ID {request.Id} not found.");
        }

        entity.SourceMemberId = request.SourceMemberId;
        entity.TargetMemberId = request.TargetMemberId;
        entity.Type = request.Type;
        entity.Order = request.Order;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
