using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : IMapFrom<Member>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Member, MemberListDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.LastName} {s.FirstName}"));
    }
}