using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Queries.GetRecentActivities;

/// <summary>
/// Query to fetch recent user activities.
/// </summary>
public record GetRecentActivitiesQuery : IRequest<Result<PaginatedList<UserActivityDto>>>
{
    /// <summary>
    /// The maximum number of activities to return.
    /// </summary>
    public TargetType? TargetType { get; init; }

    /// <summary>
    /// Optional: Filter activities by the ID of the target resource.
    /// </summary>
    public string? TargetId { get; init; }

    /// <summary>
    /// Optional: Filter activities by GroupId (e.g., FamilyId).
    /// </summary>
    public Guid? GroupId { get; init; }
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
}
