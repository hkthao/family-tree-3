using AutoMapper;
using backend.Application.Families;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Family, FamilyDetailDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>();
        CreateMap<Relationship, RelationshipDto>();
        CreateMap<Relationship, RelationshipListDto>();
        CreateMap<Member, RelationshipMemberDto>();
        CreateMap<UserActivity, UserActivityDto>();
    }
}
