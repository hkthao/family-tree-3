namespace backend.Application.Common.Models;

public class SearchResult
{
    public List<SearchItem> Items { get; set; } = new List<SearchItem>();
    public int TotalCount { get; set; }
}