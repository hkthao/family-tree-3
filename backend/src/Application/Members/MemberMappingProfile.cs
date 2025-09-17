using AutoMapper;
using backend.Domain.Entities;

namespace backend.Application.Members;

public class MemberMappingProfile : Profile
{
    public MemberMappingProfile()
    {
        CreateMap<Member, MemberDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId.ToString()));
    }
}
