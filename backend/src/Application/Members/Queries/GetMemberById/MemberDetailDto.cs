using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Application.Common.Dtos;
using backend.Application.Relationships.Queries; // Added

namespace backend.Application.Members.Queries.GetMemberById;

public class MemberDetailDto : BaseAuditableDto, IMapFrom<Member>
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
    public ICollection<RelationshipDto> Relationships { get; set; } = [];
}