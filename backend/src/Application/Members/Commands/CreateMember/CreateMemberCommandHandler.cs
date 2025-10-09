using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;
    private readonly IFamilyTreeService _familyTreeService;

    public CreateMemberCommandHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService, IMediator mediator, IFamilyTreeService familyTreeService)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _mediator = mediator;
        _familyTreeService = familyTreeService;
    }

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Result<Guid>.Failure("User is not authenticated.");
        }

        // If the user has the 'Admin' role, bypass family-specific access checks
        if (!_authorizationService.IsAdmin())
        {
            // For non-admin users, apply family-specific access checks
            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);

            if (currentUserProfile == null)
            {
                return Result<Guid>.Failure("User profile not found.");
            }

            // Check if the user has Manager role for the family
            if (!_authorizationService.CanManageFamily(request.FamilyId, currentUserProfile))
            {
                return Result<Guid>.Failure("Access denied. Only family managers can create members.");
            }
        }

        var entity = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Nickname = request.Nickname,
            DateOfBirth = request.DateOfBirth,
            DateOfDeath = request.DateOfDeath,
            PlaceOfBirth = request.PlaceOfBirth,
            PlaceOfDeath = request.PlaceOfDeath,
            Gender = request.Gender,
            AvatarUrl = request.AvatarUrl,
            Occupation = request.Occupation,
            Biography = request.Biography,
            FamilyId = request.FamilyId,
            IsRoot = request.IsRoot
        };

        if (request.IsRoot)
        {
            var currentRoot = await _context.Members
                .Where(m => m.FamilyId == request.FamilyId && m.IsRoot)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentRoot != null)
            {
                currentRoot.IsRoot = false;
            }
        }

        _context.Members.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Update family stats
        await _familyTreeService.UpdateFamilyStats(request.FamilyId, cancellationToken);

        // Record activity
        var currentUserProfileForActivity = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfileForActivity != null)
        {
            await _mediator.Send(new RecordActivityCommand
            {
                UserProfileId = currentUserProfileForActivity.Id,
                ActionType = UserActionType.CreateMember,
                TargetType = TargetType.Member,
                TargetId = entity.Id.ToString(),
                ActivitySummary = $"Created member '{entity.FullName}' in family '{request.FamilyId}'."
            }, cancellationToken);
        }

        return Result<Guid>.Success(entity.Id);
    }
}
