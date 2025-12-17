using backend.Application.Common.Dtos;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.ExportImport.Commands;
using backend.Application.Families.Dtos;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Commands.CreateFamilyDict;
using backend.Application.FamilyDicts.Commands.ImportFamilyDicts;
using backend.Application.FamilyDicts.Commands.UpdateFamilyDict;
using backend.Application.FamilyLinks.Queries;
using backend.Application.FamilyLocations; // Added for FamilyLocationDto and FamilyLocationListDto
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation; // Added for CreateFamilyLocationCommand
using backend.Application.FamilyLocations.Commands.UpdateFamilyLocation; // Added for UpdateFamilyLocationCommand
using backend.Application.FamilyLocations.Commands.DeleteFamilyLocation; // Added for DeleteFamilyLocationCommand
using backend.Application.FamilyMedias.DTOs;
using backend.Application.Identity.Queries; // Updated
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.MemberFaces.Common;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.PrivacyConfigurations.Queries;
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Application.UserPreferences.Queries;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;

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
            .ForMember(dest => dest.FamilyUsers, opt => opt.MapFrom(src => src.FamilyUsers));
        CreateMap<Member, MemberDto>();
        CreateMap<Member, MemberListDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null));
        CreateMap<Member, MemberDetailDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));

        //Event
        CreateMap<Event, EventDetailDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.Member)))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null))
            .ForMember(dest => dest.CalendarType, opt => opt.MapFrom(src => src.CalendarType))
            .ForMember(dest => dest.SolarDate, opt => opt.MapFrom(src => src.SolarDate))
            .ForMember(dest => dest.LunarDate, opt => opt.MapFrom(src => src.LunarDate)) // AutoMapper will use the LunarDate mapping
            .ForMember(dest => dest.RepeatRule, opt => opt.MapFrom(src => src.RepeatRule));
        CreateMap<Event, EventDto>()
            .ForMember(d => d.RelatedMembers, opt => opt.MapFrom(s => s.EventMembers.Select(em => em.Member)))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null))
            .ForMember(dest => dest.CalendarType, opt => opt.MapFrom(src => src.CalendarType))
            .ForMember(dest => dest.SolarDate, opt => opt.MapFrom(src => src.SolarDate))
            .ForMember(dest => dest.LunarDate, opt => opt.MapFrom(src => src.LunarDate))
            .ForMember(dest => dest.RepeatRule, opt => opt.MapFrom(src => src.RepeatRule));

        // LunarDate
        CreateMap<LunarDate, LunarDateDto>();

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
        CreateMap<FamilyUser, FamilyUserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

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



        CreateMap<MemberFace, MemberFaceDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null ? (src.Member.LastName + " " + src.Member.FirstName).Trim() : null))
            .ForMember(dest => dest.MemberGender, opt => opt.MapFrom(src => src.Member != null ? src.Member.Gender : null))
            .ForMember(dest => dest.MemberAvatarUrl, opt => opt.MapFrom(src => src.Member != null ? src.Member.AvatarUrl : null))
            .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.Member != null ? src.Member.FamilyId : (Guid?)null))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Member != null && src.Member.Family != null ? src.Member.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Member != null && src.Member.Family != null ? src.Member.Family.AvatarUrl : null))
            .ForMember(dest => dest.BirthYear, opt => opt.MapFrom(src => src.Member != null && src.Member.DateOfBirth.HasValue ? src.Member.DateOfBirth.Value.Year : (int?)null))
            .ForMember(dest => dest.DeathYear, opt => opt.MapFrom(src => src.Member != null && src.Member.DateOfDeath.HasValue ? src.Member.DateOfDeath.Value.Year : (int?)null));

        _ = CreateMap<Domain.ValueObjects.BoundingBox, BoundingBoxDto>();


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

        // FamilyMedia mappings
        CreateMap<FamilyMedia, FamilyMediaDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.ToString() : null)) // Placeholder, will need a resolver for actual user name
            .ForMember(dest => dest.MediaLinks, opt => opt.MapFrom(src => src.MediaLinks));

        CreateMap<MediaLink, MediaLinkDto>()
            .ForMember(dest => dest.RefName, opt => opt.Ignore()); // Placeholder, will need a resolver to get name from Member/MemberStory



        // FamilyLocation mappings
        CreateMap<FamilyLocation, FamilyLocationDto>();
        CreateMap<FamilyLocation, FamilyLocationListDto>();
        CreateMap<CreateFamilyLocationCommand, FamilyLocation>();
        CreateMap<UpdateFamilyLocationCommand, FamilyLocation>();
    }
}