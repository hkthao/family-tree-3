namespace backend.Application.Families.Queries;

/// <summary>
/// DTO đại diện cho cấu hình giới hạn của một gia đình.
/// </summary>
public class FamilyLimitConfigurationDto
{
    /// <summary>
    /// ID của cấu hình giới hạn.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID của gia đình mà cấu hình này thuộc về.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Số lượng thành viên tối đa được phép trong gia đình.
    /// </summary>
    public int MaxMembers { get; set; }

    /// <summary>
    /// Dung lượng lưu trữ tối đa được cấp cho gia đình (tính bằng Megabyte).
    /// </summary>
    public int MaxStorageMb { get; set; }

    /// <summary>
    /// Giới hạn số lượng yêu cầu trò chuyện AI mỗi tháng cho gia đình.
    /// </summary>
    public int AiChatMonthlyLimit { get; set; }

    /// <summary>
    /// Số lượng yêu cầu trò chuyện AI đã sử dụng trong tháng hiện tại.
    /// </summary>
    public int AiChatMonthlyUsage { get; set; }
}
