using backend.Application.Events;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.ExportImport.Commands; // New using statement
using backend.Application.Families.Dtos; // New using statement
using backend.Application.Families.Queries;
// Removed: using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
// Removed: using backend.Application.Families.Queries.GetFamilyDetails;
using backend.Application.FamilyDicts; // New using statement
using backend.Application.FamilyDicts.Commands.CreateFamilyDict; // New using statement
using backend.Application.FamilyDicts.Commands.ImportFamilyDicts; // New using statement
using backend.Application.FamilyDicts.Commands.UpdateFamilyDict; // New using statement
using backend.Application.FamilyLinks.Queries;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.MemberFaces.Common; // NEW
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.MemberStories.Commands.CreateMemberStory; // NEW
using backend.Application.MemberStories.Commands.UpdateMemberStory; // NEW
using backend.Application.MemberStories.DTOs; // NEW
using backend.Application.PdfTemplates.Dtos; // Added for PdfTemplateDto
using backend.Application.PrivacyConfigurations.Queries;
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
        // Removed: CreateMap<Family, FamilyListDto>();
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null));
        CreateMap<Member, MemberDetailDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));

        //Event
        CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.MemberId)))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.Member)))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));

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
        CreateMap<NamesByRegionCommandDto, NamesByRegion>();
        CreateMap<NamesByRegionUpdateCommandDto, NamesByRegion>();
        CreateMap<NamesByRegionImportDto, NamesByRegion>();
        CreateMap<FamilyDictImportDto, FamilyDict>();

        // Export/Import DTOs
        CreateMap<Family, FamilyExportDto>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
            .ForMember(dest => dest.Relationships, opt => opt.MapFrom(src => src.Relationships))
            .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.Events));
        CreateMap<Member, MemberExportDto>();
        CreateMap<Relationship, RelationshipExportDto>();
        CreateMap<Event, EventExportDto>()
            .ForMember(dest => dest.RelatedMembers, opt => opt.MapFrom(src => src.EventMembers.Select(em => em.MemberId)));

        // PdfTemplate DTO
        CreateMap<PdfTemplate, PdfTemplateDto>();
        CreateMap<MemberStory, MemberStoryDto>()
            .ForMember(dest => dest.MemberFullName, opt => opt.MapFrom(src => src.Member != null ? src.Member.FullName : string.Empty))
            .ForMember(dest => dest.MemberAvatarUrl, opt => opt.MapFrom(src => src.Member != null ? src.Member.AvatarUrl : null))
            .ForMember(dest => dest.MemberGender, opt => opt.MapFrom(src => src.Member != null ? src.Member.Gender : null))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created));
        CreateMap<CreateMemberStoryCommand, MemberStory>();
        CreateMap<UpdateMemberStoryCommand, MemberStory>();

        CreateMap<MemberFace, MemberFaceDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Member != null ? src.Member.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Member != null ? src.Member.Family.AvatarUrl : null));

        // PrivacyConfiguration mapping (already exists, ensuring no duplication)
        CreateMap<PrivacyConfiguration, PrivacyConfigurationDto>()
            .ForMember(dest => dest.PublicMemberProperties,
                       opt => opt.MapFrom(src => src.GetPublicMemberPropertiesList()));

        CreateMap<FamilyLinkRequest, FamilyLinkRequestDto>()
            .ForMember(dest => dest.RequestingFamilyName, opt => opt.MapFrom(src => src.RequestingFamily.Name))
            .ForMember(dest => dest.TargetFamilyName, opt => opt.MapFrom(src => src.TargetFamily.Name));

        CreateMap<FamilyLink, FamilyLinkDto>()
            .ForMember(dest => dest.Family1Name, opt => opt.MapFrom(src => src.Family1.Name))
            .ForMember(dest => dest.Family2Name, opt => opt.MapFrom(src => src.Family2.Name));
    }
}
