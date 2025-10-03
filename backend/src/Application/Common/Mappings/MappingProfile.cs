using backend.Application.Events;
using backend.Domain.Entities;
using backend.Application.Families;
using backend.Application.Members;

namespace backend.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.RelatedMembers.Select(m => m.Id)));

        CreateMap<Family, FamilyDto>();
        CreateMap<Member, MemberDto>();
    }


}