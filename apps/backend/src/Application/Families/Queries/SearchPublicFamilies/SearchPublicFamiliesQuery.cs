using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

/// <summary>
/// Truy vấn để tìm kiếm các gia đình công khai.
/// </summary>
public record SearchPublicFamiliesQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyDto>>>
{
    /// <summary>
    /// Thuật ngữ tìm kiếm để lọc các gia đình theo tên hoặc mô tả.
    /// </summary>
    public string? SearchTerm { get; init; }
}
