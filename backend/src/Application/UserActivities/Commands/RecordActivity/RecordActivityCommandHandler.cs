using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace backend.Application.UserActivities.Commands.RecordActivity;

/// <summary>
/// Handler for recording a user activity.
/// </summary>
public class RecordActivityCommandHandler : IRequestHandler<RecordActivityCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RecordActivityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

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

        var entity = new UserActivity
        {
            UserProfileId = request.UserProfileId,
            ActionType = request.ActionType,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            GroupId = groupId,
            Metadata = request.Metadata,
            ActivitySummary = request.ActivitySummary,
        };

        _context.UserActivities.Add(entity);

        // SaveChangesAsync is awaited to ensure the activity is recorded.
        // For true fire-and-forget, this could be wrapped in a background task,
        // but for audit purposes, we generally want to ensure it's persisted.
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
