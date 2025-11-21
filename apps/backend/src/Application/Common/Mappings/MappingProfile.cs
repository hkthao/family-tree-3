using backend.Application.Events;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.Families;
using backend.Application.Families.Dtos; // New using statement
using backend.Application.Families.ExportImport; // New using statement
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.FamilyDicts; // New using statement
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Application.UserPreferences.Queries;
using backend.Application.Users.Queries;
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
        CreateMap<Family, FamilyDetailDto>()
            .ForMember(dest => dest.FamilyUsers, opt => opt.MapFrom(src => src.FamilyUsers)); // Modified mapping
        CreateMap<Family, FamilyListDto>();
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null));
        CreateMap<Member, MemberDetailDto>();

        //Event
        CreateMap<Event, EventListDto>();
        CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.MemberId)));
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.MemberId)));

        //Relationship
        CreateMap<Relationship, RelationshipDto>();
        CreateMap<Relationship, RelationshipListDto>()
            .ForMember(dest => dest.SourceMember, opt => opt.MapFrom(src => src.SourceMember))
            .ForMember(dest => dest.TargetMember, opt => opt.MapFrom(src => src.TargetMember));
        CreateMap<Member, RelationshipMemberDto>();
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<UserActivity, UserActivityDto>();
        CreateMap<UserPreference, UserPreferenceDto>();
        CreateMap<User, UserDto>();
        CreateMap<FamilyUser, FamilyUserDto>() // New mapping
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? (src.User.Profile != null ? src.User.Profile.Name : src.User.Email) : null));

        // FamilyDict
        CreateMap<FamilyDict, FamilyDictDto>();
        CreateMap<NamesByRegion, NamesByRegionDto>();

        // Export/Import DTOs
        CreateMap<Family, FamilyExportDto>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
            .ForMember(dest => dest.Relationships, opt => opt.MapFrom(src => src.Relationships))
            .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.Events));
        CreateMap<Member, MemberExportDto>();
        CreateMap<Relationship, RelationshipExportDto>();
        CreateMap<Event, EventExportDto>()
            .ForMember(dest => dest.RelatedMembers, opt => opt.MapFrom(src => src.EventMembers.Select(em => em.MemberId)));
    }
}
