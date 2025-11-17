using backend.Application.Common.Dtos;
using backend.Application.Relationships.Queries;
using backend.Domain.Enums; // Add this for RelationshipType

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string FullName => $"{LastName} {FirstName}";
    public string Code { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public Guid FamilyId { get; set; }
    public string? FamilyName { get; set; }
    public bool IsRoot { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? Gender { get; set; }

    public string? FatherFullName { get; set; }
    public string? FatherAvatarUrl { get; set; }
    public string? MotherFullName { get; set; }
    public string? MotherAvatarUrl { get; set; }
    public string? HusbandFullName { get; set; }
    public string? HusbandAvatarUrl { get; set; }
    public string? WifeFullName { get; set; }
    public string? WifeAvatarUrl { get; set; }

    public string? FatherGender { get; set; }
    public string? MotherGender { get; set; }
    public string? HusbandGender { get; set; }
    public string? WifeGender { get; set; }

    public string? BirthDeathYears =>
        (DateOfBirth.HasValue ? DateOfBirth.Value.Year.ToString() : "") +
        (DateOfBirth.HasValue && DateOfDeath.HasValue ? " - " : "") +
        (DateOfDeath.HasValue ? DateOfDeath.Value.Year.ToString() : "");
}
