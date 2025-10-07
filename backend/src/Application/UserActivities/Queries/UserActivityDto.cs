using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Application.UserActivities.Queries;

public class UserActivityDto
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public UserActionType ActionType { get; set; }
    public TargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string ActivitySummary { get; set; } = null!;
    public DateTime Created { get; set; }
}
