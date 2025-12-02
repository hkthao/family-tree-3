using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandHandler(
    IApplicationDbContext context,
    IAuthorizationService authorizationService) : IRequestHandler<UpdateMemberBiographyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateMemberBiographyCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync([request.MemberId], cancellationToken);
        if (member == null)
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        var canAccess = _authorizationService.CanAccessFamily(member.FamilyId);
        if (!canAccess)
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        member.UpdateBiography(request.BiographyContent);
        member.AddDomainEvent(new Domain.Events.Members.MemberBiographyUpdatedEvent(member));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
