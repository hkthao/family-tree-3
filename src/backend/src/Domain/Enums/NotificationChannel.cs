namespace backend.Domain.Enums;

/// <summary>
/// Kênh gửi thông báo.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Thông báo trong ứng dụng (Web và Mobile UI).
    /// </summary>
    InApp = 0,

    /// <summary>
    /// Thông báo đẩy qua Firebase cho thiết bị di động.
    /// </summary>
    Firebase = 1,

    /// <summary>
    /// Thông báo qua Email.
    /// </summary>
    Email = 2,

    /// <summary>
    /// Thông báo qua SMS.
    /// </summary>
    SMS = 3,

    /// <summary>
    /// Thông báo qua Webhook.
    /// </summary>
    Webhook = 4
}
