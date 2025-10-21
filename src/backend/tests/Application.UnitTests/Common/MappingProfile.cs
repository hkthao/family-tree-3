using AutoMapper;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDetailDto>();
    }
}
