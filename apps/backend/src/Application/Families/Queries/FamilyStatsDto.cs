namespace backend.Application.Families.Queries;

/// <summary>
/// DTO chứa thông tin thống kê về số lượng thành viên và số thế hệ của một gia đình.
/// </summary>
public class FamilyStatsDto
{
    /// <summary>
    /// Tổng số thành viên trong gia đình.
    /// </summary>
    public int TotalMembers { get; set; }

    /// <summary>
    /// Tổng số thế hệ trong gia đình.
    /// </summary>
    public int TotalGenerations { get; set; }
}