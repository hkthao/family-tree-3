namespace backend.Application.Common.Models;

public class PaginatedList<T>(List<T> items, int count, int page, int itemsPerPage)
{
    public List<T> Items { get; set; } = items;
    public int Page { get; set; } = page;
    public int TotalPages { get; set; } = (int)Math.Ceiling(count / (double)itemsPerPage);
    public int TotalItems { get; set; } = count;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int itemsPerPage)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToListAsync();

        return new PaginatedList<T>(items, count, page, itemsPerPage);
    }
}
