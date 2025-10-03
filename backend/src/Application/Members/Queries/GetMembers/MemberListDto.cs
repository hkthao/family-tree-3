using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Application.Common.Dtos;

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : BaseAuditableDto, IMapFrom<Member>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public ICollection<RelationshipDto> Relationships { get; set; } = new List<RelationshipDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Member, MemberListDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.LastName} {s.FirstName}"));
    }
}