using backend.Application.Families.Queries;
using backend.Domain.Entities;

namespace backend.Application.Families.DTOs;

/// <summary>
/// Cấu hình AutoMapper cho FamilyLimitConfiguration.
/// </summary>
public class FamilyLimitConfigurationMapperProfile : Profile
{
    public FamilyLimitConfigurationMapperProfile()
    {
        CreateMap<FamilyLimitConfiguration, FamilyLimitConfigurationDto>();
    }
}
