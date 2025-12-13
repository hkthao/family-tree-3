using backend.Application.Common.Dtos;
using backend.Application.Families.Dtos;

namespace backend.Application.Families.Queries.GetFamilyById;

public class FamilyDetailDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = null!;
    public int TotalMembers { get; set; }
    public int TotalGenerations { get; set; }
    public ICollection<FamilyUserDto> FamilyUsers { get; set; } = [];
    public List<Guid> ManagerIds
    {
        get
        {
            return [.. FamilyUsers.Where(e => e.Role == Domain.Enums.FamilyRole.Manager).Select(e => e.UserId)];
        }
    }
    public List<Guid> ViewerIds
    {
        get
        {
            return [.. FamilyUsers.Where(e => e.Role == Domain.Enums.FamilyRole.Viewer).Select(e => e.UserId)];
        }
    }
}
