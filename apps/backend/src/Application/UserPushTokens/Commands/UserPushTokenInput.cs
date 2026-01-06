namespace backend.Application.UserPushTokens.Commands;

public class UserPushTokenInput
{
    public string UserId { get; set; } = null!;
    public string ExpoPushToken { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}