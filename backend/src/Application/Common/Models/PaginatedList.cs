namespace backend.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalItems = count;
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}