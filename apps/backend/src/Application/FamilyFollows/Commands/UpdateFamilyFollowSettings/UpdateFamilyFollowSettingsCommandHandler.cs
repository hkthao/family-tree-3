using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Common.Security;
using backend.Domain.Events;

namespace backend.Application.FamilyFollows.Commands.UpdateFamilyFollowSettings;

[Authorize]
public class UpdateFamilyFollowSettingsCommandHandler : IRequestHandler<UpdateFamilyFollowSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateFamilyFollowSettingsCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateFamilyFollowSettingsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        // Find the family follow entry for the current user and family
        var familyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.UserId == currentUserId && ff.FamilyId == request.FamilyId, cancellationToken);

        if (familyFollow == null)
        {
            return Result.Failure($"User {currentUserId} is not following family {request.FamilyId}.");
        }

        // Update the new boolean properties directly
        familyFollow.NotifyDeathAnniversary = request.NotifyDeathAnniversary;
        familyFollow.NotifyBirthday = request.NotifyBirthday;
        familyFollow.NotifyEvent = request.NotifyEvent;

        familyFollow.AddDomainEvent(new FamilyFollowSettingsUpdatedEvent(familyFollow));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
