using AutoMapper;
using backend.Application.Families;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Family, FamilyDetailDto>();
    }
}
