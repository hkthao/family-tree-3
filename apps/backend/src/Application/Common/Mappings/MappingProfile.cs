using backend.Application.Events;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.Families;
using backend.Application.Families.Dtos; // New using statement
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Application.UserPreferences.Queries;
using backend.Application.Users.Queries;
using backend.Domain.Entities;
using backend.Domain.Enums;

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
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FatherFullName, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Father && r.SourceMember != null)!.SourceMember!.FullName))
            .ForMember(dest => dest.FatherAvatarUrl, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Father && r.SourceMember != null)!.SourceMember!.AvatarUrl))
            .ForMember(dest => dest.MotherFullName, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Mother && r.SourceMember != null)!.SourceMember!.FullName))
            .ForMember(dest => dest.MotherAvatarUrl, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Mother && r.SourceMember != null)!.SourceMember!.AvatarUrl))
            .ForMember(dest => dest.HusbandFullName, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Husband && r.TargetMember != null)!.TargetMember!.FullName))
            .ForMember(dest => dest.HusbandAvatarUrl, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Husband && r.TargetMember != null)!.TargetMember!.AvatarUrl))
            .ForMember(dest => dest.WifeFullName, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Wife && r.TargetMember != null)!.TargetMember!.FullName))
            .ForMember(dest => dest.WifeAvatarUrl, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Wife && r.TargetMember != null)!.TargetMember!.AvatarUrl))
            .ForMember(dest => dest.FatherGender, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Father && r.SourceMember != null)!.SourceMember!.Gender))
            .ForMember(dest => dest.MotherGender, opt => opt.MapFrom(src => src.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Mother && r.SourceMember != null)!.SourceMember!.Gender))
            .ForMember(dest => dest.HusbandGender, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Husband && r.TargetMember != null)!.TargetMember!.Gender))
            .ForMember(dest => dest.WifeGender, opt => opt.MapFrom(src => src.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Wife && r.TargetMember != null)!.TargetMember!.Gender));
        CreateMap<Member, MemberDetailDto>();
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
        CreateMap<User, UserDto>();
        CreateMap<FamilyUser, FamilyUserDto>() // New mapping
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
    }
}
