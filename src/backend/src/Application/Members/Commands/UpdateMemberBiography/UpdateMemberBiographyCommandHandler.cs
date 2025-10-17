using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;

namespace backend.Application.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandHandler : IRequestHandler<UpdateMemberBiographyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly FamilyAuthorizationService _familyAuthorizationService;

    public UpdateMemberBiographyCommandHandler(
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

    public async Task<Result> Handle(UpdateMemberBiographyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result.Failure("User is not authenticated.", "Authentication");
        }

        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
        }

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        var authorizationResult = await _familyAuthorizationService.AuthorizeFamilyAccess(member.FamilyId, cancellationToken);
        if (!authorizationResult.IsSuccess)
        {
            return Result.Failure(authorizationResult.Error ?? "Unknown authorization error.", authorizationResult.ErrorSource ?? "Authorization");
        }

        member.Biography = request.BiographyContent;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
