using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.GetMembers;

public class MemberFilterModel : PaginationModel
{
    public string? SearchQuery { get; set; }
    public string? Gender { get; set; }
    public Guid? FamilyId { get;  set; }
    public Guid[] Ids { get; set; } = [];
}
