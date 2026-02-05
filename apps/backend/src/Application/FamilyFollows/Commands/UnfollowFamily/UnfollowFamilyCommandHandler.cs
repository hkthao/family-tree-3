using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Common.Security;
using backend.Domain.Events;

namespace backend.Application.FamilyFollows.Commands.UnfollowFamily;

[Authorize]
public class UnfollowFamilyCommandHandler : IRequestHandler<UnfollowFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UnfollowFamilyCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UnfollowFamilyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        // Find the family follow entry for the current user and family
        var familyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.UserId == currentUserId && ff.FamilyId == request.FamilyId, cancellationToken);

        if (familyFollow == null)
        {
            return Result.Failure($"User {currentUserId} is not following family {request.FamilyId}.");
        }



        _context.FamilyFollows.Remove(familyFollow);

        // Add domain event for successful unfollow
        familyFollow.AddDomainEvent(new FamilyFollowDeletedEvent(familyFollow));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
