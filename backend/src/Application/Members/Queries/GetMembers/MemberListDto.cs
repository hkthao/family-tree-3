using backend.Application.Common.Dtos;
using backend.Application.Relationships.Queries; // Added

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public Guid FamilyId { get; set; }
    public bool IsRoot { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public ICollection<RelationshipDto> Relationships { get; set; } = [];
}
