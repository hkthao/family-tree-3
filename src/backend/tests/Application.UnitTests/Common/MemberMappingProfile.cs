using AutoMapper;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class MemberMappingProfile : Profile
{
    public MemberMappingProfile()
    {
        CreateMap<Member, MemberDetailDto>();
    }
}
