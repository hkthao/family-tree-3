using backend.Application.Common.Dtos;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.ExportImport.Commands;
using backend.Application.Families.Dtos;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.GetPrivacyConfiguration;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Commands.CreateFamilyDict;
using backend.Application.FamilyDicts.Commands.ImportFamilyDicts;
using backend.Application.FamilyDicts.Commands.UpdateFamilyDict;
using backend.Application.FamilyLinks.Queries;
using backend.Application.FamilyLocations; // Added for FamilyLocationDto and FamilyLocationListDto
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation; // Added for CreateFamilyLocationCommand
using backend.Application.FamilyLocations.Commands.ImportFamilyLocations; // Added
using backend.Application.FamilyLocations.Commands.UpdateFamilyLocation; // Added for UpdateFamilyLocationCommand
using backend.Application.FamilyMedias.DTOs;
using backend.Application.Identity.Queries; // Updated
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.MemberFaces.Commands.ImportMemberFaces; // Added
using backend.Application.MemberFaces.Common;
using backend.Application.Members.DTOs; // Added for MemberImportDto
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.MemoryItems.Commands.CreateMemoryItem; // Added
using backend.Application.MemoryItems.Commands.UpdateMemoryItem; // Added
using backend.Application.MemoryItems.DTOs; // Added
using backend.Application.Prompts.Commands.ImportPrompts;
using backend.Application.Prompts.DTOs;
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

        CreateMap<Prompt, PromptDto>();
        CreateMap<ImportPromptItemDto, Prompt>();

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
        CreateMap<ImportMemberFaceItemDto, MemberFace>(); // Added

        _ = CreateMap<Domain.ValueObjects.BoundingBox, BoundingBoxDto>();


        // PrivacyConfiguration mapping (already exists, ensuring no duplication)
        CreateMap<PrivacyConfiguration, PrivacyConfigurationDto>()
            .ForMember(dest => dest.PublicMemberProperties,
                       opt => opt.MapFrom(src => src.GetPublicMemberPropertiesList()))
            .ForMember(dest => dest.PublicEventProperties,
                       opt => opt.MapFrom(src => src.GetPublicEventPropertiesList()))
            .ForMember(dest => dest.PublicFamilyProperties,
                       opt => opt.MapFrom(src => src.GetPublicFamilyPropertiesList()))
            .ForMember(dest => dest.PublicFamilyLocationProperties,
                       opt => opt.MapFrom(src => src.GetPublicFamilyLocationPropertiesList()))
            .ForMember(dest => dest.PublicMemoryItemProperties,
                       opt => opt.MapFrom(src => src.GetPublicMemoryItemPropertiesList()))
            .ForMember(dest => dest.PublicMemberFaceProperties,
                       opt => opt.MapFrom(src => src.GetPublicMemberFacePropertiesList()))
            .ForMember(dest => dest.PublicFoundFaceProperties,
                       opt => opt.MapFrom(src => src.GetPublicFoundFacePropertiesList()));



        CreateMap<FamilyLink, FamilyLinkDto>()
            .ForMember(dest => dest.Family1Name, opt => opt.MapFrom(src => src.Family1.Name))
            .ForMember(dest => dest.Family2Name, opt => opt.MapFrom(src => src.Family2.Name));

        // New mapping for PaginatedList<Family> to PaginatedList<FamilyDto>
        CreateMap<PaginatedList<Family>, PaginatedList<FamilyDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));

        // FamilyMedia mappings
        CreateMap<FamilyMedia, FamilyMediaDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.ToString() : null)) // Placeholder, will need a resolver for actual user name
            .ForMember(dest => dest.MediaLinks, opt => opt.MapFrom(src => src.MediaLinks));

        CreateMap<MediaLink, MediaLinkDto>()
            .ForMember(dest => dest.RefName, opt => opt.Ignore()); // Placeholder, will need a resolver to get name from Member/MemberStory



        // FamilyLocation mappings
        CreateMap<FamilyLocation, FamilyLocationDto>();
        CreateMap<CreateFamilyLocationCommand, FamilyLocation>();
        CreateMap<UpdateFamilyLocationCommand, FamilyLocation>();
        CreateMap<ImportFamilyLocationItemDto, FamilyLocation>(); // Added

        // MemoryItem mappings
        CreateMap<MemoryItem, MemoryItemDto>()
            .ForMember(dest => dest.MemoryMedia, opt => opt.MapFrom(src => src.MemoryMedia))
            .ForMember(dest => dest.MemoryPersons, opt => opt.MapFrom(src => src.MemoryPersons));
        CreateMap<MemoryMedia, MemoryMediaDto>();
        CreateMap<MemoryPerson, MemoryPersonDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null ? src.Member.FullName : null));

        CreateMap<FamilyLimitConfiguration, FamilyLimitConfigurationDto>();
        CreateMap<CreateMemoryItemCommand, MemoryItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MemoryMedia, opt => opt.Ignore())
            .ForMember(dest => dest.MemoryPersons, opt => opt.Ignore());
        CreateMap<UpdateMemoryItemCommand, MemoryItem>()
            .ForMember(dest => dest.MemoryMedia, opt => opt.Ignore())
            .ForMember(dest => dest.MemoryPersons, opt => opt.Ignore());
        CreateMap<CreateMemoryMediaCommandDto, MemoryMedia>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MemoryItem, opt => opt.Ignore());
        CreateMap<UpdateMemoryMediaCommandDto, MemoryMedia>()
            .ForMember(dest => dest.MemoryItem, opt => opt.Ignore());

        // Voice Profile mappings
        CreateMap<Domain.Entities.VoiceProfile, Application.VoiceProfiles.Queries.VoiceProfileDto>();

        // Voice Generation mappings
        CreateMap<Domain.Entities.VoiceGeneration, Application.VoiceGenerations.Queries.VoiceGenerationDto>();

        // Event mappings for importing
        CreateMap<EventDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID will be generated
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.EventMembers, opt => opt.Ignore()) // Handled manually in handler
            .ForMember(dest => dest.SolarDate, opt => opt.Ignore()) // Handled manually in handler (using SetSolarDate)
            .ForMember(dest => dest.LunarDate, opt => opt.Ignore()); // Handled manually in handler (using SetLunarDate)
        CreateMap<LunarDateDto, LunarDate>();

        // Member mappings for importing
        CreateMap<MemberImportDto, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID will be generated
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.Ignore()) // Calculated property
            .ForMember(dest => dest.FatherId, opt => opt.Ignore()) // Handled manually in handler
            .ForMember(dest => dest.MotherId, opt => opt.Ignore()) // Handled manually in handler
            .ForMember(dest => dest.HusbandId, opt => opt.Ignore()) // Handled manually in handler
            .ForMember(dest => dest.WifeId, opt => opt.Ignore()); // Handled manually in handler
    }
}
