using backend.Domain.Entities;

namespace backend.Application.Members.Queries;

public class MemberDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Generation { get; set; }
    public Guid FamilyId { get; set; }
    public string? Biography { get; set; }
    public string? Metadata { get; set; }

    // Relationships (using DTOs to avoid circular references)
    public MemberLookupDto? Father { get; set; }
    public MemberLookupDto? Mother { get; set; }
    public MemberLookupDto? Spouse { get; set; }
    public ICollection<MemberLookupDto> Children { get; set; } = new List<MemberLookupDto>();

    // Manual mapping from Member entity to MemberDto
    public static MemberDto FromEntity(Member member)
    {
        return new MemberDto
        {
            Id = member.Id,
            FullName = member.FullName,
            DateOfBirth = member.DateOfBirth,
            DateOfDeath = member.DateOfDeath,
            PlaceOfBirth = member.PlaceOfBirth,
            PlaceOfDeath = member.PlaceOfDeath,
            Gender = member.Gender,
            AvatarUrl = member.AvatarUrl,
            Phone = member.Phone,
            Email = member.Email,
            Generation = member.Generation,
            FamilyId = member.FamilyId,
            Biography = member.Biography,
            Metadata = member.Metadata,
            Father = member.Father != null ? new MemberLookupDto { Id = member.Father.Id, FullName = member.Father.FullName } : null,
            Mother = member.Mother != null ? new MemberLookupDto { Id = member.Mother.Id, FullName = member.Mother.FullName } : null,
            Spouse = member.Spouse != null ? new MemberLookupDto { Id = member.Spouse.Id, FullName = member.Spouse.FullName } : null,
            Children = member.Children.Select(c => new MemberLookupDto { Id = c.Id, FullName = c.FullName }).ToList()
        };
    }
}