namespace backend.Application.Common.Models;

/// <summary>
/// Đại diện cho một danh sách dữ liệu được phân trang.
/// </summary>
/// <typeparam name="T">Kiểu của các mục trong danh sách.</typeparam>
public class PaginatedList<T>(List<T> items, int count, int page, int itemsPerPage)
{
    /// <summary>
    /// Danh sách các mục trên trang hiện tại.
    /// </summary>
    public List<T> Items { get; set; } = items;
    /// <summary>
    /// Số trang hiện tại.
    /// </summary>
    public int Page { get; set; } = page;
    /// <summary>
    /// Tổng số trang.
    /// </summary>
    public int TotalPages { get; set; } = (int)Math.Ceiling(count / (double)itemsPerPage);
    /// <summary>
    /// Tổng số mục trên tất cả các trang.
    /// </summary>
    public int TotalItems { get; set; } = count;

    /// <summary>
    /// Tạo một thể hiện mới của PaginatedList một cách không đồng bộ.
    /// </summary>
    /// <param name="source">Nguồn IQueryable để phân trang.</param>
    /// <param name="page">Số trang hiện tại.</param>
    /// <param name="itemsPerPage">Số lượng mục trên mỗi trang.</param>
    /// <returns>Một Task chứa PaginatedList của kiểu T.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int itemsPerPage)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToListAsync();

        return new PaginatedList<T>(items, count, page, itemsPerPage);
    }
    /// <summary>
    /// Tạo một PaginatedList rỗng.
    /// </summary>
    /// <returns>Một PaginatedList rỗng của kiểu T.</returns>
    public static PaginatedList<T> Empty()
    {
        return new PaginatedList<T>(new List<T>(), 0, 1, 10); // Default to page 1, 10 items per page
    }
}
