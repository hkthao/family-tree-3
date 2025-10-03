using backend.Application.Events;
using backend.Domain.Entities;
using backend.Application.Families;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Events.Queries.GetEvents;

namespace backend.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.RelatedMembers.Select(m => m.Id)));

        CreateMap<Family, FamilyDto>();
        CreateMap<Member, MemberListDto>();
        CreateMap<Member, MemberDetailDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Family, FamilyDetailDto>();
        CreateMap<Event, EventListDto>();
    }
}