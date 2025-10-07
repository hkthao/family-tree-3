using MediatR;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Application.UserActivities.Commands.RecordActivity;

/// <summary>
/// Command to record a user activity.
/// </summary>
public record RecordActivityCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// The ID of the UserProfile who performed the action.
    /// </summary>
    public Guid UserProfileId { get; init; }

    /// <summary>
    /// The type of action performed (e.g., CreateFamily, UpdateMember, Login).
    /// </summary>
    public UserActionType ActionType { get; init; }

    /// <summary>
    /// The type of the target resource (e.g., Family, Member, UserProfile).
    /// </summary>
    public TargetType TargetType { get; init; }

    /// <summary>
    /// The ID of the target resource (e.g., FamilyId, MemberId).
    /// </summary>
    public string? TargetId { get; init; }

    /// <summary>
    /// Optional metadata in JSON format for storing additional details about the action.
    /// </summary>
    public JsonDocument? Metadata { get; init; }

    /// <summary>
    /// Summary of the activity for display purposes.
    /// </summary>
    public string ActivitySummary { get; init; } = null!;
}
