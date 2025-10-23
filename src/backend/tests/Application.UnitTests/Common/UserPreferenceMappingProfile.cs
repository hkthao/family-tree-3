using AutoMapper;
using backend.Application.UserPreferences.Queries;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class UserPreferenceMappingProfile : Profile
{
    public UserPreferenceMappingProfile()
    {
        CreateMap<UserPreference, UserPreferenceDto>();
    }
}
