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
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? HusbandId { get; set; }
    public Guid? WifeId { get; set; }
    public ICollection<RelationshipDto> SourceRelationships { get; set; } = [];
    public ICollection<RelationshipDto> TargetRelationships { get; set; } = [];
    public string? BirthDeathYears =>
}
