using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Members;

public class MemberFilterModel
{
    public string? SearchQuery { get; set; }
    public string? Gender { get; set; }
    public Guid? FamilyId { get;  set; }
    public Guid[] Ids { get; set; } = [];
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}
