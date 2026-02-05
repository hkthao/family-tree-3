using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Family;
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

        // 3. Validate that member cannot be related to themselves
        if (request.FatherId.HasValue && request.FatherId.Value == request.MemberId)
            return Result<Guid>.Failure("Member cannot be their own father.", ErrorSources.Validation);
        if (request.MotherId.HasValue && request.MotherId.Value == request.MemberId)
            return Result<Guid>.Failure("Member cannot be their own mother.", ErrorSources.Validation);
        if (request.HusbandId.HasValue && request.HusbandId.Value == request.MemberId)
            return Result<Guid>.Failure("Member cannot be their own husband.", ErrorSources.Validation);
        if (request.WifeId.HasValue && request.WifeId.Value == request.MemberId)
            return Result<Guid>.Failure("Member cannot be their own wife.", ErrorSources.Validation);

        // 4. Update Relationships
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
