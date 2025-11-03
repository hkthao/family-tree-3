using System.Text.Json;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Queries;

public class UserActivityDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserActionType ActionType { get; set; }
    public TargetType TargetType { get; set; }
    public string TargetId { get; set; } = null!;
    public Guid? GroupId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string ActivitySummary { get; set; } = null!;
    public DateTime Created { get; set; }
}
