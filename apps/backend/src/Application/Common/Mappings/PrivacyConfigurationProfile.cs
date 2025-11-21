using backend.Application.PrivacyConfigurations.Queries;
using backend.Domain.Entities;

namespace backend.Application.Common.Mappings;

public class PrivacyConfigurationProfile : Profile
{
    public PrivacyConfigurationProfile()
    {
        CreateMap<PrivacyConfiguration, PrivacyConfigurationDto>()
            .ForMember(dest => dest.PublicMemberProperties, opt => opt.MapFrom(src => src.GetPublicMemberPropertiesList()));
    }
}
