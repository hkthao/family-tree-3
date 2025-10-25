using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser user) : IRequestHandler<DeleteRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUser _user = user;

    public async Task<Result<bool>> Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        if (!_user.Id.HasValue)
            return Result<bool>.Failure("User is not authenticated.", "Authentication");

        var entity = await _context.Relationships.FindAsync(request.Id);
        if (entity == null)
            return Result<bool>.Failure($"Relationship with ID {request.Id} not found.");

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(entity.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<bool>.Failure($"Source member for relationship {request.Id} not found.", "NotFound");
        }

        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
        {
            return Result<bool>.Failure("Access denied. Only family managers or admins can delete relationships.", "Forbidden");
        }

        entity.AddDomainEvent(new RelationshipDeletedEvent(entity));
        _context.Relationships.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
