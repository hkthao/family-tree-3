using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries;

public class FamilyDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    public string? Address { get; set; }
    public string? GenealogyRecord { get; set; }
    public string? ProgenitorName { get; set; }
    public string? FamilyCovenant { get; set; }
    public string? ContactInfo { get; set; }
    public int TotalMembers { get; set; } = 0;
    public int? TotalGenerations { get; set; } = null;
    public string? Visibility { get; set; }
    public string? AvatarUrl { get; set; }
    public string Source { get; set; } = null!;
    public bool IsVerified { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public List<FamilyUserDto> FamilyUsers { get; set; } = [];
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
    public bool IsFollowing { get; set; } = false; // NEW
}
