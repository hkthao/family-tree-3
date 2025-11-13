namespace backend.Application.Common.Models;

/// <summary>
/// Lớp cơ sở trừu tượng cho các truy vấn có phân trang.
/// </summary>
public abstract record PaginatedQuery
{
    /// <summary>
    /// Số trang hiện tại. Mặc định là 1.
    /// </summary>
    public int Page { get; init; } = 1;
    /// <summary>
    /// Số lượng mục trên mỗi trang. Mặc định là 10.
    /// </summary>
    public int ItemsPerPage { get; init; } = 10;
    /// <summary>
    /// Tên trường để sắp xếp kết quả.
    /// </summary>
    public string? SortBy { get; init; }
    /// <summary>
    /// Thứ tự sắp xếp (ví dụ: "asc" cho tăng dần, "desc" cho giảm dần).
    /// </summary>
    public string? SortOrder { get; init; }
}
