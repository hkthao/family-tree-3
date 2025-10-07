using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Application.UserActivities.Commands.RecordActivity;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;

    public UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<Guid>.Failure("User profile not found.", "NotFound");
        }

        // Authorization check (similar to CreateMemberCommandHandler)
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(request.FamilyId, currentUserProfile))
        {
            return Result<Guid>.Failure("Access denied. Only family managers can update members.", "Forbidden");
        }

        var entity = await _context.Members
            .Include(m => m.Relationships)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        var oldFullName = entity.FullName; // Capture old name for activity summary

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Nickname = request.Nickname;
        entity.DateOfBirth = request.DateOfBirth;
        entity.DateOfDeath = request.DateOfDeath;
        entity.PlaceOfBirth = request.PlaceOfBirth;
        entity.PlaceOfDeath = request.PlaceOfDeath;
        entity.Gender = request.Gender;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Occupation = request.Occupation;
        entity.Biography = request.Biography;
        entity.FamilyId = request.FamilyId;
        entity.IsRoot = request.IsRoot;

        if (request.IsRoot)
        {
            var currentRoot = await _context.Members
                .Where(m => m.FamilyId == request.FamilyId && m.IsRoot && m.Id != request.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentRoot != null)
            {
                currentRoot.IsRoot = false;
            }
        }

        // Manage Relationships
        var existingRelationships = entity.Relationships.ToList();
        var incomingRelationships = request.Relationships.ToList();

        // Remove relationships explicitly marked for deletion
        foreach (var deletedRelId in request.DeletedRelationshipIds)
        {
            var relToRemove = existingRelationships.FirstOrDefault(er => er.Id == deletedRelId);
            if (relToRemove != null)
            {
                _context.Relationships.Remove(relToRemove);
            }
        }

        // Add or update relationships from the incoming list
        foreach (var incomingRel in incomingRelationships)
        {
            var existingRel = existingRelationships.FirstOrDefault(er => er.Id == incomingRel.Id);

            if (existingRel == null) // New relationship
            {
                _context.Relationships.Add(new Relationship
                {
                    SourceMemberId = entity.Id,
                    TargetMemberId = incomingRel.TargetMemberId,
                    Type = incomingRel.Type,
                    Order = incomingRel.Order
                });
            }
            else // Update existing relationship
            {
                existingRel.TargetMemberId = incomingRel.TargetMemberId;
                existingRel.Type = incomingRel.Type;
                existingRel.Order = incomingRel.Order;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = currentUserProfile.Id,
            ActionType = UserActionType.UpdateMember,
            TargetType = TargetType.Member,
            TargetId = entity.Id.ToString(),
            ActivitySummary = $"Updated member '{oldFullName}' to '{entity.FullName}'."
        }, cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
