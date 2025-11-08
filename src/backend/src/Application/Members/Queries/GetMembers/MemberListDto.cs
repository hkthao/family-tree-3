using backend.Application.Common.Dtos;
using backend.Application.Relationships.Queries;

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyName { get; set; }
    public bool IsRoot { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? Gender { get; set; }
    public string? BirthDeathYears =>
        (DateOfBirth.HasValue ? DateOfBirth.Value.Year.ToString() : "") +
        (DateOfBirth.HasValue && DateOfDeath.HasValue ? " - " : "") +
        (DateOfDeath.HasValue ? DateOfDeath.Value.Year.ToString() : "");
    public ICollection<RelationshipDto> Relationships { get; set; } = [];
}
