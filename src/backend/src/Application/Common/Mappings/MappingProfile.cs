using backend.Application.Events;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.Families;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Application.UserPreferences.Queries;
using backend.Domain.Entities;

namespace backend.Application.Common.Mappings;

/// <summary>
/// Định nghĩa các cấu hình ánh xạ cho AutoMapper.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp MappingProfile và định nghĩa các ánh xạ giữa các thực thể và DTO.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Family, FamilyDetailDto>();
        CreateMap<Family, FamilyListDto>();
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.BirthDeathYears, opt => opt.MapFrom(src =>
                (src.DateOfBirth.HasValue ? src.DateOfBirth.Value.Year.ToString() : "") +
                (src.DateOfBirth.HasValue && src.DateOfDeath.HasValue ? " - " : "") +
                (src.DateOfDeath.HasValue ? src.DateOfDeath.Value.Year.ToString() : "")
            ));
        CreateMap<Member, MemberDetailDto>()
            .ForMember(dest => dest.BirthDeathYears, opt => opt.MapFrom(src =>
                (src.DateOfBirth.HasValue ? src.DateOfBirth.Value.Year.ToString() : "") +
                (src.DateOfBirth.HasValue && src.DateOfDeath.HasValue ? " - " : "") +
                (src.DateOfDeath.HasValue ? src.DateOfDeath.Value.Year.ToString() : "")
            ));
        CreateMap<Event, EventListDto>();
        CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.MemberId)));
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.MemberId)));
        CreateMap<Relationship, RelationshipDto>();
        CreateMap<Relationship, RelationshipListDto>()
            .ForMember(dest => dest.SourceMember, opt => opt.MapFrom(src => src.SourceMember))
            .ForMember(dest => dest.TargetMember, opt => opt.MapFrom(src => src.TargetMember));
        CreateMap<Member, RelationshipMemberDto>();
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<UserActivity, UserActivityDto>();
        CreateMap<UserPreference, UserPreferenceDto>();
    }
}
