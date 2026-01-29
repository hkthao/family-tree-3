namespace backend.Application.Common.Events;

public record UserSyncEvent(
    string UserId,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Avatar,
    string? Locale,
    string? Timezone
);

public record SaveExpoPushTokenEvent(
    string UserId,
    List<string?> ExpoPushTokens
);

public record DeleteExpoPushTokenEvent(
    string UserId,
    string? ExpoPushToken
);

public record SendNotificationEvent(
    string WorkflowId,
    string UserId,
    object? Payload
);
