using AutoMapper;
using backend.Application.Families;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Family, FamilyDetailDto>();

        CreateMap<Member, MemberListDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null));
    }
}
