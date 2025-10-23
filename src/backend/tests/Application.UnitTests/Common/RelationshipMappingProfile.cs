using AutoMapper;
using backend.Application.Relationships.Queries;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class RelationshipMappingProfile : Profile
{
    public RelationshipMappingProfile()
    {
        CreateMap<Relationship, RelationshipDto>();
    }
}
