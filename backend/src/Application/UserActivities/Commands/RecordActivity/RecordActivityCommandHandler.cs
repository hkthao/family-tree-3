using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
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
        var entity = new UserActivity
        {
            UserProfileId = request.UserProfileId,
            ActionType = request.ActionType,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
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
