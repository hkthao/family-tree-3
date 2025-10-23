using AutoMapper;
using backend.Application.Relationships.Queries;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class RelationshipMemberMappingProfile : Profile
{
    public RelationshipMemberMappingProfile()
    {
        CreateMap<Member, RelationshipMemberDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName));
    }
}
