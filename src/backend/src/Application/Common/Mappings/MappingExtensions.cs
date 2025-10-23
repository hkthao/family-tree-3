using backend.Application.Common.Models;

namespace backend.Application.Common.Mappings;

/// <summary>
/// Cung cấp các phương thức mở rộng để ánh xạ và phân trang các truy vấn.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Tạo một danh sách phân trang (PaginatedList) từ một IQueryable.
    /// </summary>
    /// <typeparam name="TDestination">Kiểu đích của danh sách phân trang.</typeparam>
    /// <param name="queryable">IQueryable nguồn.</param>
    /// <param name="page">Số trang hiện tại.</param>
    /// <param name="itemsPerPage">Số lượng mục trên mỗi trang.</param>
    /// <returns>Một Task chứa PaginatedList của TDestination.</returns>
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int page, int itemsPerPage) where TDestination : class
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), page, itemsPerPage);

    /// <summary>
    /// Ánh xạ một IQueryable sang một danh sách các đối tượng TDestination và trả về không đồng bộ.
    /// </summary>
    /// <typeparam name="TDestination">Kiểu đích của danh sách.</typeparam>
    /// <param name="queryable">IQueryable nguồn.</param>
    /// <param name="configuration">Cấu hình ánh xạ.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một Task chứa danh sách các đối tượng TDestination.</returns>
    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(this IQueryable queryable, IConfigurationProvider configuration, CancellationToken cancellationToken = default) where TDestination : class
        => queryable.ProjectTo<TDestination>(configuration).AsNoTracking().ToListAsync(cancellationToken);
}
