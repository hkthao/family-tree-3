namespace backend.Application.Common.Models;

public class GlobalSearchResult
{
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DeepLink { get; set; } = null!;
    public double Score { get; set; }
}
