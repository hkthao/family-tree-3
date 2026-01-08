using System.Text.Json; // Added
using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một User Aggregate Root trong hệ thống, được xác định bởi nhà cung cấp xác thực (ví dụ: Auth0).
/// Nó quản lý các thực thể con như UserProfile.
/// Một người dùng có thể có nhiều hồ sơ (UserProfile) liên quan.
/// </summary>
public class User : BaseAuditableEntity, IAggregateRoot
{
    /// <summary>
    /// ID duy nhất của người dùng từ nhà cung cấp xác thực (ví dụ: Auth0 'sub' claim).
    /// </summary>
    public string AuthProviderId { get; set; } = null!;

    /// <summary>
    /// ID của người đăng ký trên Novu Cloud để gửi/nhận thông báo.
    /// </summary>
    public string? SubscriberId { get; set; }

    /// <summary>
    /// Địa chỉ email của người dùng.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Hồ sơ (UserProfile) thuộc về người dùng này.
    /// </summary>
    public UserProfile? Profile { get; private set; } // Make setter private

    /// <summary>
    /// Tùy chọn người dùng (UserPreference) thuộc về người dùng này.
    /// </summary>
    public UserPreference? Preference { get; private set; }

    /// <summary>
    /// Danh sách các hoạt động của người dùng (UserActivity) thuộc về người dùng này.
    /// </summary>
    private readonly HashSet<UserActivity> _userActivities = new();
    public IReadOnlyCollection<UserActivity> UserActivities => _userActivities;

    /// <summary>
    /// Danh sách các token đẩy (push token) của người dùng (UserPushToken) thuộc về người dùng này.
    /// </summary>
    private readonly HashSet<UserPushToken> _userPushTokens = new();
    public IReadOnlyCollection<UserPushToken> UserPushTokens => _userPushTokens;

    // Private constructor for EF Core
    private User() { }

    public User(string authProviderId, string email)
    {
        AuthProviderId = authProviderId;
        Email = email;
        Profile = new UserProfile(Id); // Initialize with default profile
        Preference = new UserPreference(Id); // Initialize with default preference
    }

    public User(string authProviderId, string email, string name, string? firstName, string? lastName, string? phone, string? avatar)
    {
        AuthProviderId = authProviderId;
        Email = email;
        Profile = new UserProfile(Id, authProviderId, email, name, firstName, lastName, phone, avatar); // Initialize with default profile
        Preference = new UserPreference(Id); // Initialize with default preference
    }

    public UserActivity AddUserActivity(string activityType, string description, TargetType targetType = TargetType.None, string? targetId = null, Guid? groupId = null, JsonDocument? metadata = null)
    {
        var _userActivity = new UserActivity(Id, activityType, description, targetType, targetId, groupId, metadata);
        _userActivities.Add(_userActivity);
        return _userActivity;
    }

    public UserProfile? UpdateProfile(string externalId, string email, string name, string firstName, string lastName, string phone, string avatar)
    {
        if (Profile == null)
        {
            throw new InvalidOperationException("User profile does not exist.");
        }

        Profile.Update(externalId, email, name, firstName, lastName, phone, avatar);
        return Profile;
    }

    public void UpdatePreference(Theme theme, Language language)
    {
        if (Preference == null)
        {
            throw new InvalidOperationException("User preference does not exist.");
        }

        Preference.Update(theme, language);
    }
}
