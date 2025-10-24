namespace backend.Domain.Enums;

/// <summary>
/// Loại thông báo.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Thông báo chung, không thuộc loại cụ thể nào.
    /// </summary>
    General = 0,

    /// <summary>
    /// Thông báo khi có thành viên gia đình mới được thêm.
    /// </summary>
    NewFamilyMember = 1,

    /// <summary>
    /// Thông báo khi một mối quan hệ được xác nhận.
    /// </summary>
    RelationshipConfirmed = 2,

    /// <summary>
    /// Thông báo khi người dùng được mời vào một gia đình hoặc sự kiện.
    /// </summary>
    UserInvited = 3,

    /// <summary>
    /// Cảnh báo hệ thống hoặc thông báo quan trọng từ quản trị viên.
    /// </summary>
    SystemAlert = 4,

    /// <summary>
    /// Thông báo liên quan đến sinh nhật hoặc các sự kiện kỷ niệm khác.
    /// </summary>
    BirthdayReminder = 5,

    /// <summary>
    /// Thông báo khi có cập nhật về hồ sơ thành viên.
    /// </summary>
    MemberProfileUpdate = 6,

    /// <summary>
    /// Thông báo khi một gia đình mới được tạo.
    /// </summary>
    FamilyCreated = 7,

    /// <summary>
    /// Thông báo khi thông tin gia đình được cập nhật.
    /// </summary>
    FamilyUpdated = 8,

    /// <summary>
    /// Thông báo khi một gia đình bị xóa.
    /// </summary>
    FamilyDeleted = 9,

    /// <summary>
    /// Thông báo khi một thành viên mới được tạo.
    /// </summary>
    MemberCreated = 10,

    /// <summary>
    /// Thông báo khi thông tin thành viên được cập nhật.
    /// </summary>
    MemberUpdated = 11,

    /// <summary>
    /// Thông báo khi một thành viên bị xóa.
    /// </summary>
    MemberDeleted = 12,

    /// <summary>
    /// Thông báo khi tiểu sử thành viên được cập nhật.
    /// </summary>
    MemberBiographyUpdated = 13,

    /// <summary>
    /// Thông báo khi một mối quan hệ mới được tạo.
    /// </summary>
    RelationshipCreated = 14,

    /// <summary>
    /// Thông báo khi một mối quan hệ được cập nhật.
    /// </summary>
    RelationshipUpdated = 15,

    /// <summary>
    /// Thông báo khi một mối quan hệ bị xóa.
    /// </summary>
    RelationshipDeleted = 16
}
