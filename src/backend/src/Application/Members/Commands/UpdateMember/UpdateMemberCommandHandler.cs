using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, IFamilyTreeService familyTreeService) : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

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
            return Result<Guid>.Failure($"Member with ID {request.Id} not found.", "NotFound");
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

        entity.AddDomainEvent(new MemberUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        // Update family stats
        await _familyTreeService.UpdateFamilyStats(request.FamilyId, cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
