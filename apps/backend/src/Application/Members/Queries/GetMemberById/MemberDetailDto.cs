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
    public Guid FamilyId { get; set; }
    public string? Biography { get; set; }
    public bool IsRoot { get; set; }
    public string? BirthDeathYears =>
        (DateOfBirth.HasValue ? DateOfBirth.Value.Year.ToString() : "") +
        (DateOfBirth.HasValue && DateOfDeath.HasValue ? " - " : "") +
        (DateOfDeath.HasValue ? DateOfDeath.Value.Year.ToString() : "")
    ;
    public Guid? FatherId => TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Father)?.SourceMemberId;
    public Guid? MotherId => TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Mother)?.SourceMemberId;
    public Guid? HusbandId => SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Husband)?.TargetMemberId;
    public Guid? WifeId => SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Wife)?.TargetMemberId;
    public ICollection<RelationshipDto> SourceRelationships { get; set; } = [];
    public ICollection<RelationshipDto> TargetRelationships { get; set; } = [];
}
