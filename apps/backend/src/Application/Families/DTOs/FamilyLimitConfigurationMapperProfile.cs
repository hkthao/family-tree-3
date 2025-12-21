using AutoMapper;
using backend.Domain.Entities;
using backend.Application.Families.Queries;

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
