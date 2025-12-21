namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho cấu hình giới hạn và cài đặt cho một gia đình cụ thể.
/// </summary>
public class FamilyLimitConfiguration : BaseAuditableEntity
{
    /// <summary>
    /// ID của gia đình mà cấu hình này thuộc về.
    /// </summary>
    public Guid FamilyId { get; private set; }

    /// <summary>
    /// Thuộc tính điều hướng đến thực thể Family.
    /// </summary>
    public Family Family { get; private set; } = null!;

    /// <summary>
    /// Số lượng thành viên tối đa được phép trong gia đình.
    /// </summary>
    public int MaxMembers { get; private set; } = 50; // Giá trị mặc định

    /// <summary>
    /// Dung lượng lưu trữ tối đa được cấp cho gia đình (tính bằng Megabyte).
    /// </summary>
    public int MaxStorageMb { get; private set; } = 1024; // Giá trị mặc định: 1GB

    /// <summary>
    /// Private constructor cho EF Core.
    /// </summary>
    private FamilyLimitConfiguration() { }

    /// <summary>
    /// Khởi tạo một phiên bản mới của FamilyLimitConfiguration.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    public FamilyLimitConfiguration(Guid familyId)
    {
        FamilyId = familyId;
    }

    /// <summary>
    /// Cập nhật các giới hạn cấu hình cho gia đình.
    /// </summary>
    /// <param name="maxMembers">Số lượng thành viên tối đa.</param>
    /// <param name="maxStorageMb">Dung lượng lưu trữ tối đa (MB).</param>
    public void Update(int maxMembers, int maxStorageMb)
    {
        if (maxMembers <= 0)
        {
            throw new ArgumentException("Số lượng thành viên tối đa phải lớn hơn 0.", nameof(maxMembers));
        }
        if (maxStorageMb <= 0)
        {
            throw new ArgumentException("Dung lượng lưu trữ tối đa phải lớn hơn 0.", nameof(maxStorageMb));
        }

        MaxMembers = maxMembers;
        MaxStorageMb = maxStorageMb;
    }
}
