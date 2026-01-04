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
using backend.Application.FamilyLocations;
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation;
using backend.Application.FamilyLocations.Commands.ImportFamilyLocations;
using backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.Identity.Queries;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.MemberFaces.Commands.ImportMemberFaces;
using backend.Application.MemberFaces.Common;
using backend.Application.Members.DTOs;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.MemoryItems.Commands.CreateMemoryItem;
using backend.Application.MemoryItems.Commands.UpdateMemoryItem;
using backend.Application.MemoryItems.DTOs;
using backend.Application.Prompts.Commands.ImportPrompts;
using backend.Application.Prompts.DTOs;
using backend.Application.Relationships.Queries;
using backend.Application.UserActivities.Queries;
using backend.Application.UserPreferences.Queries;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
namespace backend.Application.Common.Mappings;

public class MappingProfile : Profile
{
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
        CreateMap<Event, EventDetailDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.FamilyAvatarUrl, opt => opt.MapFrom(src => src.Family != null ? src.Family.AvatarUrl : null));
        CreateMap<EventMember, EventMemberDto>();
        CreateMap<LunarDate, LunarDateDto>();
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
        CreateMap<FamilyDict, FamilyDictDto>();
        CreateMap<NamesByRegion, NamesByRegionDto>();
        CreateMap<NamesByRegionCommandDto, NamesByRegion>();
        CreateMap<NamesByRegionUpdateCommandDto, NamesByRegion>();
        CreateMap<NamesByRegionImportDto, NamesByRegion>();
        CreateMap<FamilyDictImportDto, FamilyDict>();
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
        CreateMap<ImportMemberFaceItemDto, MemberFace>();
        _ = CreateMap<BoundingBox, BoundingBoxDto>();
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
        CreateMap<PaginatedList<Family>, PaginatedList<FamilyDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));
        CreateMap<FamilyMedia, FamilyMediaDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.ToString() : null))
            .ForMember(dest => dest.MediaLinks, opt => opt.MapFrom(src => src.MediaLinks));
        CreateMap<MediaLink, MediaLinkDto>()
            .ForMember(dest => dest.RefName, opt => opt.Ignore());
        CreateMap<FamilyLocation, FamilyLocationDto>();
        CreateMap<CreateFamilyLocationCommand, FamilyLocation>();
        CreateMap<UpdateFamilyLocationCommand, FamilyLocation>();
        CreateMap<ImportFamilyLocationItemDto, FamilyLocation>();
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
        CreateMap<VoiceProfile, VoiceProfiles.Queries.VoiceProfileDto>();
        CreateMap<VoiceGeneration, VoiceProfiles.Queries.VoiceGenerationDto>();
        CreateMap<EventDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.EventMembers, opt => opt.Ignore())
            .ForMember(dest => dest.SolarDate, opt => opt.Ignore())
            .ForMember(dest => dest.LunarDate, opt => opt.Ignore());
        CreateMap<LunarDateDto, LunarDate>();
        CreateMap<MemberImportDto, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.FatherId, opt => opt.Ignore())
            .ForMember(dest => dest.MotherId, opt => opt.Ignore())
            .ForMember(dest => dest.HusbandId, opt => opt.Ignore())
            .ForMember(dest => dest.WifeId, opt => opt.Ignore());
    }
}
