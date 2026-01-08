namespace backend.Domain.Entities;

public class UserPushToken : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string ExpoPushToken { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // Private constructor for EF Core
    private UserPushToken() { }

    public static UserPushToken Create(Guid userId, string expoPushToken, string platform, string deviceId)
    {
        return new UserPushToken
        {
            UserId = userId,
            ExpoPushToken = expoPushToken,
            Platform = platform,
            DeviceId = deviceId,
            IsActive = true // Newly created tokens are active by default
        };
    }

    public void UpdateToken(string expoPushToken, string platform, bool isActive)
    {
        ExpoPushToken = expoPushToken;
        Platform = platform;
        IsActive = isActive;
    }
}
