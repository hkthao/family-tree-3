using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Members;

public class MemberDto : IMapFrom<Member>
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Generation { get; set; }


    public string? Biography { get; set; }
    public object? Metadata { get; set; }
}
