using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<DeleteRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<bool>> Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<bool>.Failure("User profile not found.", "NotFound");
        }

        var entity = await _context.Relationships.FindAsync(request.Id);

        if (entity == null)
        {
            return Result<bool>.Failure($"Relationship with ID {request.Id} not found.");
        }

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(entity.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<bool>.Failure($"Source member for relationship {request.Id} not found.", "NotFound");
        }

        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(sourceMember.FamilyId, currentUserProfile))
        {
            return Result<bool>.Failure("Access denied. Only family managers or admins can delete relationships.", "Forbidden");
        }

        var activitySummary = $"Deleted relationship {entity.SourceMemberId}-{entity.Type}-{entity.TargetMemberId}.";

        entity.AddDomainEvent(new RelationshipDeletedEvent(entity));
        _context.Relationships.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
