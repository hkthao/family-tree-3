using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;

namespace backend.Application.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IAuthorizationService authorizationService) : IRequestHandler<UpdateMemberBiographyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateMemberBiographyCommand request, CancellationToken cancellationToken)
    {
        if (!_user.Id.HasValue)
            return Result.Failure("User is not authenticated.", "Authentication");

        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
            return Result.Failure($"Member with ID {request.MemberId} not found.", "NotFound");

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        var canAccess = _authorizationService.CanAccessFamily(member.FamilyId);
        if (!canAccess)
            return Result.Failure("Only family managers or admins can update member biography.", "Forbidden");

        member.Biography = request.BiographyContent;
        member.AddDomainEvent(new MemberBiographyUpdatedEvent(member));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
