using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<CreateRelationshipCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<Guid>> Handle(CreateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<Guid>.Failure("User profile not found.", "NotFound");
        }

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(request.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<Guid>.Failure($"Source member with ID {request.SourceMemberId} not found.", "NotFound");
        }

        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(sourceMember.FamilyId, currentUserProfile))
        {
            return Result<Guid>.Failure("Access denied. Only family managers or admins can create relationships.", "Forbidden");
        }

        var entity = new Relationship
        {
            SourceMemberId = request.SourceMemberId,
            TargetMemberId = request.TargetMemberId,
            Type = request.Type,
            Order = request.Order
        };

        _context.Relationships.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = currentUserProfile.Id,
            ActionType = UserActionType.CreateRelationship,
            TargetType = TargetType.Member,
            TargetId = entity.Id.ToString(),
            ActivitySummary = $"Created relationship between {sourceMember.FullName} and {request.TargetMemberId} (Type: {request.Type})."
        }, cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
