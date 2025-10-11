namespace backend.Application.Common.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public PaginatedList(List<T> items, int count, int page, int itemsPerPage)
        {
            Page = page;
            TotalPages = (int)Math.Ceiling(count / (double)itemsPerPage);
            TotalItems = count;
            Items = items;
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int itemsPerPage)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToListAsync();

            return new PaginatedList<T>(items, count, page, itemsPerPage);
        }
    }
}
