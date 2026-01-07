using backend.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.Common.Security;
using backend.Application.Common.Models;
using backend.Domain.Events;
using backend.Application.Common.Exceptions; // For NotFoundException, although returning Result.Failure is better

namespace backend.Application.FamilyFollows.Commands.UnfollowFamily;

[Authorize]
public class UnfollowFamilyCommandHandler : IRequestHandler<UnfollowFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnfollowFamilyCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _currentUser = currentUser;
        _domainEventDispatcher = domainEventDispatcher;
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

        // Scenario 2: User is a member of family A -> auto-follow -> cannot unfollow (only mute)
        var isMember = await _context.FamilyUsers.AnyAsync(fu => fu.UserId == currentUserId && fu.FamilyId == request.FamilyId, cancellationToken);
        if (isMember)
        {
            return Result.Failure($"User is a member of family {request.FamilyId} and cannot unfollow. Consider updating notification settings instead.");
        }

        _context.FamilyFollows.Remove(familyFollow);

        // Add domain event for successful unfollow
        familyFollow.AddDomainEvent(new FamilyFollowDeletedEvent(familyFollow));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
