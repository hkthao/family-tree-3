using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Domain.Entities;

/// <summary>
/// Represents a user activity for auditing and user feed purposes.
/// </summary>
public class UserActivity : BaseAuditableEntity
{
    /// <summary>
    /// The ID of the UserProfile who performed the action.
    /// </summary>
    public Guid UserProfileId { get; set; }

    /// <summary>
    /// Navigation property for the UserProfile.
    /// </summary>
    public UserProfile UserProfile { get; set; } = null!;

    /// <summary>
    /// The type of action performed (e.g., CreateFamily, UpdateMember, Login).
    /// </summary>
    public UserActionType ActionType { get; set; }

    /// <summary>
    /// The type of the target resource (e.g., Family, Member, UserProfile).
    /// </summary>
    public TargetType TargetType { get; set; }

    /// <summary>
    /// The ID of the target resource (e.g., FamilyId, MemberId).
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// Optional metadata in JSON format for storing additional details about the action.
    /// </summary>
    public JsonDocument? Metadata { get; set; }

    /// <summary>
    /// Summary of the activity for display purposes.
    /// </summary>
    public string ActivitySummary { get; set; } = null!;
}
