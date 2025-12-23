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
    public int MaxMembers { get; private set; } = 5000; // Giá trị mặc định

    /// <summary>
    /// Dung lượng lưu trữ tối đa được cấp cho gia đình (tính bằng Megabyte).
    /// </summary>
    public int MaxStorageMb { get; private set; } = 2048; // Giá trị mặc định: 2GB

    /// <summary>
    /// Giới hạn số lượng yêu cầu trò chuyện AI mỗi tháng cho gia đình.
    /// </summary>
    public int AiChatMonthlyLimit { get; private set; } = 100; // Giá trị mặc định: 100 lượt trò chuyện AI mỗi tháng

    /// <summary>
    /// Số lượng yêu cầu trò chuyện AI đã sử dụng trong tháng hiện tại.
    /// </summary>
    public int AiChatMonthlyUsage { get; private set; } = 0; // Giá trị mặc định: 0

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
    /// <param name="aiChatMonthlyLimit">Giới hạn số lượng yêu cầu trò chuyện AI mỗi tháng.</param>
    public void Update(int maxMembers, int maxStorageMb, int aiChatMonthlyLimit)
    {
        if (maxMembers <= 0)
        {
            throw new ArgumentException("Số lượng thành viên tối đa phải lớn hơn 0.", nameof(maxMembers));
        }
        if (maxStorageMb <= 0)
        {
            throw new ArgumentException("Dung lượng lưu trữ tối đa phải lớn hơn 0.", nameof(maxStorageMb));
        }
        if (aiChatMonthlyLimit < 0)
        {
            throw new ArgumentException("Giới hạn trò chuyện AI hàng tháng không thể âm.", nameof(aiChatMonthlyLimit));
        }

        MaxMembers = maxMembers;
        MaxStorageMb = maxStorageMb;
        AiChatMonthlyLimit = aiChatMonthlyLimit;
    }

    /// <summary>
    /// Ghi lại việc sử dụng một yêu cầu trò chuyện AI.
    /// </summary>
    public void IncrementAiChatUsage()
    {
        AiChatMonthlyUsage++;
    }

    /// <summary>
    /// Đặt lại số lượng yêu cầu trò chuyện AI đã sử dụng về 0 (thường vào đầu tháng mới).
    /// </summary>
    public void ResetAiChatUsage()
    {
        AiChatMonthlyUsage = 0;
    }
}
