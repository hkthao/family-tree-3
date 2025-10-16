using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandHandler : IRequestHandler<CreateRelationshipsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly FamilyAuthorizationService _familyAuthorizationService;

    public CreateRelationshipsCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IAuthorizationService authorizationService,
        FamilyAuthorizationService familyAuthorizationService)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _familyAuthorizationService = familyAuthorizationService;
    }

    public async Task<Result<List<Guid>>> Handle(CreateRelationshipsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<List<Guid>>.Failure("User is not authenticated.", "Authentication");
        }

        var createdRelationshipIds = new List<Guid>();

        foreach (var relationshipInput in request.Relationships) // Changed relationshipDto to relationshipInput
        {
            // Validate member existence
            var sourceMember = await _context.Members.FindAsync(new object[] { relationshipInput.SourceMemberId }, cancellationToken);
            var targetMember = await _context.Members.FindAsync(new object[] { relationshipInput.TargetMemberId }, cancellationToken);

            if (sourceMember == null)
            {
                return Result<List<Guid>>.Failure($"Source member with ID {relationshipInput.SourceMemberId} not found.", "NotFound");
            }
            if (targetMember == null)
            {
                return Result<List<Guid>>.Failure($"Target member with ID {relationshipInput.TargetMemberId} not found.", "NotFound");
            }

            // Authorization check for the family of the source member
            var authorizationResult = await _familyAuthorizationService.AuthorizeFamilyAccess(sourceMember.FamilyId, cancellationToken);
            if (!authorizationResult.IsSuccess)
            {
                return Result<List<Guid>>.Failure(authorizationResult.Error ?? "Unknown authorization error.", authorizationResult.ErrorSource ?? "Authorization");
            }

            var entity = new Relationship
            {
                SourceMemberId = relationshipInput.SourceMemberId,
                TargetMemberId = relationshipInput.TargetMemberId,
                Type = relationshipInput.Type,
                Order = relationshipInput.Order,
                StartDate = relationshipInput.StartDate,
                EndDate = relationshipInput.EndDate,
                Description = relationshipInput.Description,
                FamilyId = sourceMember.FamilyId // Assume relationship belongs to source member's family
            };

            _context.Relationships.Add(entity);
            await _context.SaveChangesAsync(cancellationToken); // Save each relationship individually for now, can optimize later

            createdRelationshipIds.Add(entity.Id);
        }

        return Result<List<Guid>>.Success(createdRelationshipIds);
    }
}
