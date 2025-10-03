using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries;

public class MemberDto : IMapFrom<Member>
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
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? SpouseId { get; set; }
}
