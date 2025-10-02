namespace backend.Application.Common.Models;

public class FamilyFilterModel
{
    public string? SearchQuery { get; set; }
    public Guid? FamilyId { get; set; }
    public string? Visibility { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 10;
}