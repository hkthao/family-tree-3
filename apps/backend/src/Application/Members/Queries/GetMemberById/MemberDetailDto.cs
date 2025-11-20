using backend.Application.Common.Dtos;
using backend.Application.Relationships.Queries;
using backend.Domain.Enums; // Add this for RelationshipType

namespace backend.Application.Members.Queries.GetMemberById;

public class MemberDetailDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string FullName => $"{LastName} {FirstName}";
    public string? Nickname { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Occupation { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid FamilyId { get; set; }
    public string? Biography { get; set; }
    public bool IsRoot { get; set; }
    public string? BirthDeathYears =>
        (DateOfBirth.HasValue ? DateOfBirth.Value.Year.ToString() : "") +
        (DateOfBirth.HasValue && DateOfDeath.HasValue ? " - " : "") +
        (DateOfDeath.HasValue ? DateOfDeath.Value.Year.ToString() : "")
    ;
    public string? FatherFullName { get; set; }
    public string? MotherFullName { get; set; }
    public string? HusbandFullName { get; set; }
    public string? WifeFullName { get; set; }
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? HusbandId { get; set; }
    public Guid? WifeId { get; set; }
    public ICollection<RelationshipDto> SourceRelationships { get; set; } = [];
    public ICollection<RelationshipDto> TargetRelationships { get; set; } = [];
}
