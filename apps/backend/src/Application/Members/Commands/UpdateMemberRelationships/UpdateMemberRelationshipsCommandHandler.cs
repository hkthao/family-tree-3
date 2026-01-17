using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateMemberRelationships;

public class UpdateMemberRelationshipsCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMemberRelationshipService memberRelationshipService) : IRequestHandler<UpdateMemberRelationshipsCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;

    public async Task<Result<Guid>> Handle(UpdateMemberRelationshipsCommand request, CancellationToken cancellationToken)
    {
        // 1. Authorization Check
        if (!_authorizationService.CanManageFamily(request.FamilyId))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        // 2. Validate Member exists
        var member = await _context.Members.FindAsync([request.MemberId], cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        // 3. Update Relationships
        await _memberRelationshipService.UpdateMemberRelationshipsAsync(
            request.MemberId,
            request.FatherId,
            request.MotherId,
            request.HusbandId,
            request.WifeId,
            cancellationToken
        );

        // 4. Save changes to context. This is important as UpdateMemberRelationshipsAsync modifies entities in context but doesn't save.
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(request.MemberId);
    }
}
