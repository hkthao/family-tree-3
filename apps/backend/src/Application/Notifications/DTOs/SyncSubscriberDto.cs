namespace backend.Application.Notifications.DTOs;

public record SyncSubscriberDto
{
    public string UserId { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Avatar { get; init; }
    public string? Locale { get; init; }
    public string? Timezone { get; init; }
}
