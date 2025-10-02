using backend.Domain.Enums;

namespace backend.Application.Common.Models;

public class MemberFilterModel
{
    public string? Gender { get; set; }
    public Guid? FamilyId { get; set; }
    public string? SearchQuery { get; set; }
    public List<Guid>? Ids { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 10;
}