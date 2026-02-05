using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Common.Security;
using backend.Domain.Entities;
using backend.Domain.Events;

namespace backend.Application.FamilyFollows.Commands.FollowFamily;

[Authorize]
public class FollowFamilyCommandHandler : IRequestHandler<FollowFamilyCommand, Result<Guid>> // Changed return type
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public FollowFamilyCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(FollowFamilyCommand request, CancellationToken cancellationToken) // Changed return type
    {
        var currentUserId = _currentUser.UserId;

        // Check if family exists
        var family = await _context.Families.FindAsync(new object[] { request.FamilyId }, cancellationToken);
        if (family == null)
        {
            return Result<Guid>.Failure($"Family with ID {request.FamilyId} not found.");
        }

        // Check if already following
        var existingFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.UserId == currentUserId && ff.FamilyId == request.FamilyId, cancellationToken);

        if (existingFollow != null)
        {
            return Result<Guid>.Failure($"User {currentUserId} is already following family {request.FamilyId}.");
        }

        // Check if user is a member of the family (Scenario 2 from task.md)
        var isMember = await _context.FamilyUsers.AnyAsync(fu => fu.UserId == currentUserId && fu.FamilyId == request.FamilyId, cancellationToken);

        var entity = FamilyFollow.Create(currentUserId, request.FamilyId);
        entity.NotifyDeathAnniversary = request.NotifyDeathAnniversary;
        entity.NotifyBirthday = request.NotifyBirthday;
        entity.NotifyEvent = request.NotifyEvent;

        _context.FamilyFollows.Add(entity);

        // Add domain event for successful follow
        entity.AddDomainEvent(new FamilyFollowCreatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id); // Return success result
    }
}
