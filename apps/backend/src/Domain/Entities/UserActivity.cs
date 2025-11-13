using System.Text.Json;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Represents a user activity for auditing and user feed purposes.
/// </summary>
public class UserActivity : BaseAuditableEntity
{
    /// <summary>
    /// The ID of the User who performed the action.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Navigation property for the User.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The type of action performed (e.g., CreateFamily, UpdateMember, Login).
    /// </summary>
    public UserActionType ActionType { get; private set; }

    /// <summary>
    /// The type of the target resource (e.g., Family, Member, UserProfile).
    /// </summary>
    public TargetType TargetType { get; private set; }

    /// <summary>
    /// The ID of the target resource (e.g., FamilyId, MemberId).
    /// </summary>
    public string? TargetId { get; private set; }

    /// <summary>
    /// Optional: The ID of the group (e.g., FamilyId) related to the activity.
    /// </summary>
    public Guid? GroupId { get; private set; }

    /// <summary>
    /// Optional metadata in JSON format for storing additional details about the action.
    /// </summary>
    public JsonDocument? Metadata { get; private set; }

    /// <summary>
    /// Summary of the activity for display purposes.
    /// </summary>
    public string ActivitySummary { get; private set; } = null!;

    // Private constructor for EF Core
    private UserActivity() { }

    public UserActivity(Guid userId, string actionType, string activitySummary, TargetType targetType, string? targetId, Guid? groupId, JsonDocument? metadata)
    {
        UserId = userId;
        ActionType = Enum.Parse<UserActionType>(actionType);
        ActivitySummary = activitySummary;
        TargetType = targetType;
        TargetId = targetId;
        GroupId = groupId;
        Metadata = metadata;
    }
}
