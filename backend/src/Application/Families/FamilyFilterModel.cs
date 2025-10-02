using backend.Application.Common.Models;

namespace backend.Application.Families;

public class FamilyFilterModel : PaginationModel
{
    public string? Keyword { get; set; }
}
