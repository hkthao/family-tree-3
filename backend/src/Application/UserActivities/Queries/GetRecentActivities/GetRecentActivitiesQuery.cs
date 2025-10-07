using MediatR;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Queries.GetRecentActivities;

/// <summary>
/// Query to fetch recent user activities.
/// </summary>
public record GetRecentActivitiesQuery : IRequest<Result<List<UserActivityDto>>>
{
    /// <summary>
    /// The maximum number of activities to return.
    /// </summary>
    public int Limit { get; init; } = 20;

    /// <summary>
    /// Optional: Filter activities by the type of the target resource.
    /// </summary>
    public TargetType? TargetType { get; init; }

    /// <summary>
    /// Optional: Filter activities by the ID of the target resource.
    /// </summary>
    public Guid? TargetId { get; init; }

    /// <summary>
    /// Optional: Filter activities by FamilyId (requires UserActivity to have FamilyId or join).
    /// </summary>
    public Guid? FamilyId { get; init; }
}
