namespace backend.Application.UserPushTokens.Commands;

public record UpdateUserPushTokenInput
{
    public Guid UserId { get; set; }
    public string ExpoPushToken { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public bool IsActive { get; set; }
}
