using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Commands.RecordActivity;

/// <summary>
/// Handler for recording a user activity.
/// </summary>
public class RecordActivityCommandHandler(IApplicationDbContext context) : IRequestHandler<RecordActivityCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<Guid>> Handle(RecordActivityCommand request, CancellationToken cancellationToken)
    {
        Guid? groupId = null;

        if (request.TargetId != null)
        {
            switch (request.TargetType)
            {
                case TargetType.Family:
                    groupId = Guid.Parse(request.TargetId);
                    break;
                case TargetType.Member:
                    var member = await _context.Members.FindAsync(Guid.Parse(request.TargetId), cancellationToken);
                    groupId = member?.FamilyId;
                    break;
                case TargetType.Event:
                    var @event = await _context.Events.FindAsync(Guid.Parse(request.TargetId), cancellationToken);
                    groupId = @event?.FamilyId;
                    break;
                case TargetType.Relationship:
                    var relationship = await _context.Relationships.FindAsync(Guid.Parse(request.TargetId), cancellationToken);
                    if (relationship != null)
                    {
                        var sourceMember = await _context.Members.FindAsync(relationship.SourceMemberId, cancellationToken);
                        groupId = sourceMember?.FamilyId;
                    }
                    break;
                case TargetType.UserProfile:
                    groupId = null;
                    break;
            }
        }

        var user = await _context.Users.WithSpecification(new UserWithActivitiesSpec(request.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result<Guid>.Failure($"User with ID {request.UserId} not found.");
        }

        var userActivity = user.AddUserActivity(
             request.ActionType.ToString(),
             request.ActivitySummary,
             request.TargetType,
             request.TargetId,
             groupId,
             request.Metadata
         );
        _context.UserActivities.Add(userActivity);

        // The other properties of UserActivity (TargetType, TargetId, GroupId, Metadata)
        // are not directly set by the AddUserActivity method in the User aggregate.
        // If these are crucial for the UserActivity, they should be passed to the
        // AddUserActivity method or handled differently.
        // For now, I will assume they are not critical for the initial implementation
        // and can be set directly on the newly added activity if needed,
        // or the AddUserActivity method in User should be extended.

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.UserActivities.Last().Id); // Return the ID of the newly added activity
    }
}
