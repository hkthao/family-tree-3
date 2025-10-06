using backend.Application.Events;
using backend.Domain.Entities;
using backend.Application.Families;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Members.Queries;
using backend.Application.Relationships.Queries; // Added

namespace backend.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Family, FamilyDetailDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>();
        CreateMap<Member, MemberDetailDto>();
        CreateMap<Event, EventListDto>();
        CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.RelatedMembers.Select(m => m.Id)));
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.RelatedMembers.Select(m => m.Id)));
        CreateMap<Relationship, RelationshipDto>();
        CreateMap<Relationship, RelationshipListDto>()
            .ForMember(dest => dest.SourceMember, opt => opt.MapFrom(src => src.SourceMember))
            .ForMember(dest => dest.TargetMember, opt => opt.MapFrom(src => src.TargetMember));
        CreateMap<Member, RelationshipMemberDto>();
    }
}