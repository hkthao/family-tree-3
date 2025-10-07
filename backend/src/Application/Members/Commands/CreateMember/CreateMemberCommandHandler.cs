using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;
using backend.Application.UserActivities.Commands.RecordActivity;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;

    public CreateMemberCommandHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService, IMediator mediator)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Result<Guid>.Failure("User is not authenticated.");
        }

        // If the user has the 'Admin' role, bypass family-specific access checks
        if (_authorizationService.IsAdmin())
        {
            // Admin can create members in any family, no further checks needed
        }
        else
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

        foreach (var relDto in request.Relationships)
        {
            entity.Relationships.Add(new Relationship
            {
                SourceMemberId = entity.Id,
                TargetMemberId = relDto.TargetMemberId,
                Type = relDto.Type,
                Order = relDto.Order
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

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