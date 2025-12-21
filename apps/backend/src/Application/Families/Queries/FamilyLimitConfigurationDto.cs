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
}
