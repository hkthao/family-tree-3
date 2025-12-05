using backend.Application.FamilyLinks.Queries;
using backend.Domain.Entities;

namespace backend.Application.FamilyLinks;

public class FamilyLinkMappingProfile : Profile
{
    public FamilyLinkMappingProfile()
    {
        CreateMap<FamilyLinkRequest, FamilyLinkRequestDto>()
            .ForMember(dest => dest.RequestingFamilyName, opt => opt.MapFrom(src => src.RequestingFamily.Name))
            .ForMember(dest => dest.TargetFamilyName, opt => opt.MapFrom(src => src.TargetFamily.Name));

        CreateMap<FamilyLink, FamilyLinkDto>()
            .ForMember(dest => dest.Family1Name, opt => opt.MapFrom(src => src.Family1.Name))
            .ForMember(dest => dest.Family2Name, opt => opt.MapFrom(src => src.Family2.Name));
    }
}