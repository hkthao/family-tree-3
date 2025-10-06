
using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Queries;

public class RelationshipMemberDto : IMapFrom<Member>
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
