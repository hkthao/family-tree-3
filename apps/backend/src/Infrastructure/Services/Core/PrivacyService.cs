using System.Reflection;
using AutoMapper;
using backend.Application.Common.Interfaces; // For ICurrentUserService
using backend.Application.Events.Queries; // For EventDto
using backend.Application.Events.Queries.GetEventById; // For EventDetailDto
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Families.Queries.GetFamilyById; // For FamilyDetailDto
using backend.Application.FamilyLocations; // For FamilyLocationDto
using backend.Application.MemberFaces.Common; // For MemberFaceDto
using backend.Application.MemberFaces.Queries.SearchVectorFace; // For FoundFaceDto
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById; // For MemberDetailDto
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Application.MemoryItems.DTOs; // For MemoryItemDto
using backend.Domain.Entities;
using backend.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class PrivacyService : IPrivacyService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    // Cache PropertyInfo objects to improve performance
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo?>> _propertyCache = [];

    public PrivacyService(IApplicationDbContext context, ICurrentUser currentUserService, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUserService;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    /// <summary>
    /// Phương thức trợ giúp chung để lọc các thuộc tính của DTO.
    /// </summary>
    /// <typeparam name="T">Loại DTO.</typeparam>
    /// <param name="sourceDto">DTO nguồn.</param>
    /// <param name="allowedProps">Danh sách các tên thuộc tính được phép (từ cấu hình quyền riêng tư).</param>
    /// <param name="alwaysProps">Danh sách các tên thuộc tính luôn được bao gồm.</param>
    /// <returns>Một thể hiện mới của DTO với các thuộc tính đã được lọc.</returns>
    private T FilterDto<T>(T sourceDto, IEnumerable<string> allowedProps, IEnumerable<string> alwaysProps) where T : new()
    {
        var filteredDto = new T();
        var sourceType = typeof(T);
        var isAnyPropertyFiltered = false;

        // Lấy hoặc thêm các thuộc tính của loại vào bộ nhớ đệm
        if (!_propertyCache.TryGetValue(sourceType, out var typeProperties))
        {
            typeProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => (PropertyInfo?)p, StringComparer.OrdinalIgnoreCase);
            _propertyCache.TryAdd(sourceType, typeProperties);
        }

        // Collect all properties that are explicitly allowed or always included
        var explicitlyIncludedProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var propName in alwaysProps)
        {
            explicitlyIncludedProps.Add(propName);
            if (typeProperties.TryGetValue(propName, out var propertyInfo) && propertyInfo != null && propertyInfo.CanWrite)
            {
                var value = propertyInfo.GetValue(sourceDto);
                propertyInfo.SetValue(filteredDto, value);
            }
        }

        foreach (var propName in allowedProps)
        {
            if (explicitlyIncludedProps.Add(propName)) // Add only if not already present
            {
                if (typeProperties.TryGetValue(propName, out var propertyInfo) && propertyInfo != null && propertyInfo.CanWrite)
                {
                    var value = propertyInfo.GetValue(sourceDto);
                    propertyInfo.SetValue(filteredDto, value);
                }
            }
        }

        // Determine if any property was filtered out
        foreach (var propInfo in typeProperties.Values)
        {
            if (propInfo != null && propInfo.CanRead && propInfo.CanWrite)
            {
                // If a property is not explicitly included and is not 'IsPrivate' itself, it means it was filtered out
                if (!explicitlyIncludedProps.Contains(propInfo.Name) && propInfo.Name != "IsPrivate")
                {
                    isAnyPropertyFiltered = true;
                    break;
                }
            }
        }

        // Set IsPrivate flag if applicable
        if (isAnyPropertyFiltered)
        {
            var isPrivateProperty = filteredDto.GetType().GetProperty("IsPrivate");
            if (isPrivateProperty != null && isPrivateProperty.CanWrite && isPrivateProperty.PropertyType == typeof(bool))
            {
                isPrivateProperty.SetValue(filteredDto, true);
            }
        }

        return filteredDto;
    }

    public async Task<MemberDto> ApplyPrivacyFilter(MemberDto memberDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin system always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberDto;
        }

        // Get family visibility
        var family = await _context.Families
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null)
        {
            // If family not found, return empty DTO or throw exception based on desired behavior
            // For now, let's return the original DTO assuming it's an error state that should be handled elsewhere.
            return memberDto;
        }

        // If family is public, or user can access the family (family admin/viewer), show full data
        if (family.Visibility == "Public" || _authorizationService.CanAccessFamily(familyId))
        {
            return memberDto;
        }

        // Otherwise, apply privacy configuration for private families
        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(PrivacyConstants.DefaultPublicMemberProperties.MemberDto);
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeMemberProps.Id,
            PrivacyConstants.AlwaysIncludeMemberProps.FirstName,
            PrivacyConstants.AlwaysIncludeMemberProps.LastName,
            PrivacyConstants.AlwaysIncludeMemberProps.FamilyId,
            PrivacyConstants.AlwaysIncludeMemberProps.Code,
            PrivacyConstants.AlwaysIncludeMemberProps.IsRoot,
            PrivacyConstants.AlwaysIncludeMemberProps.AvatarUrl
        };

        return FilterDto(memberDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<MemberDto>> ApplyPrivacyFilter(List<MemberDto> memberDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<MemberDto>();
        foreach (var memberDto in memberDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(memberDto, familyId, cancellationToken));
        }
        return filteredList;
    }

    public async Task<MemberDetailDto> ApplyPrivacyFilter(MemberDetailDto memberDetailDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin system always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberDetailDto;
        }

        // Get family visibility
        var family = await _context.Families
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null)
        {
            // If family not found, return empty DTO or throw exception based on desired behavior
            return memberDetailDto;
        }

        // If family is public, or user can access the family (family admin/viewer), show full data
        if (family.Visibility == "Public" || _authorizationService.CanAccessFamily(familyId))
        {
            return memberDetailDto;
        }

        // Otherwise, apply privacy configuration for private families
        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(PrivacyConstants.DefaultPublicMemberProperties.MemberDetailDto);
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeMemberProps.Id,
            PrivacyConstants.AlwaysIncludeMemberProps.FirstName,
            PrivacyConstants.AlwaysIncludeMemberProps.LastName,
            PrivacyConstants.AlwaysIncludeMemberProps.FamilyId,
            PrivacyConstants.AlwaysIncludeMemberProps.IsRoot,
            PrivacyConstants.AlwaysIncludeMemberProps.AvatarUrl,
            PrivacyConstants.AlwaysIncludeMemberProps.SourceRelationships,
            PrivacyConstants.AlwaysIncludeMemberProps.TargetRelationships
        };

        return FilterDto(memberDetailDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<MemberListDto> ApplyPrivacyFilter(MemberListDto memberListDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin system always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberListDto;
        }

        // Get family visibility
        var family = await _context.Families
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null)
        {
            // If family not found, return empty DTO or throw exception based on desired behavior
            return memberListDto;
        }

        // If family is public, or user can access the family (family admin/viewer), show full data
        if (family.Visibility == "Public" || _authorizationService.CanAccessFamily(familyId))
        {
            return memberListDto;
        }

        // Otherwise, apply privacy configuration for private families
        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(PrivacyConstants.DefaultPublicMemberProperties.MemberListDto);
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeMemberProps.Id,
            PrivacyConstants.AlwaysIncludeMemberProps.FirstName,
            PrivacyConstants.AlwaysIncludeMemberProps.LastName,
            PrivacyConstants.AlwaysIncludeMemberProps.Code,
            PrivacyConstants.AlwaysIncludeMemberProps.IsRoot,
            PrivacyConstants.AlwaysIncludeMemberProps.AvatarUrl,
            PrivacyConstants.AlwaysIncludeMemberProps.FamilyId,
            PrivacyConstants.AlwaysIncludeMemberProps.FamilyName,
            PrivacyConstants.AlwaysIncludeMemberProps.FatherId,
            PrivacyConstants.AlwaysIncludeMemberProps.MotherId,
            PrivacyConstants.AlwaysIncludeMemberProps.HusbandId,
            PrivacyConstants.AlwaysIncludeMemberProps.WifeId
        };

        return FilterDto(memberListDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<MemberListDto>> ApplyPrivacyFilter(List<MemberListDto> memberListDtos, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(memberListDtos);
        // var filteredList = new List<MemberListDto>();
        // foreach (var memberListDto in memberListDtos)
        // {
        //     filteredList.Add(await ApplyPrivacyFilter(memberListDto, familyId, cancellationToken));
        // }
        // return filteredList;
    }

    public async Task<EventDto> ApplyPrivacyFilter(EventDto eventDto, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(eventDto);

        // Admin always sees full data
        // if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        // {
        //     return eventDto;
        // }

        // PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        // if (privacyConfig == null)
        // {
        //     // If no config, create a default privacy config with predefined public properties
        //     privacyConfig = new PrivacyConfiguration(familyId);
        //     privacyConfig.UpdatePublicEventProperties(PrivacyConstants.DefaultPublicEventProperties.EventDto);
        // }

        // var publicProperties = privacyConfig.GetPublicEventPropertiesList();
        // var alwaysIncludeProps = new List<string>
        // {
        //     PrivacyConstants.AlwaysIncludeEventProps.Id,
        //     PrivacyConstants.AlwaysIncludeEventProps.FamilyId,
        //     PrivacyConstants.AlwaysIncludeEventProps.FamilyName,
        //     PrivacyConstants.AlwaysIncludeEventProps.FamilyAvatarUrl,
        //     PrivacyConstants.AlwaysIncludeEventProps.RelatedMembers,
        //     PrivacyConstants.AlwaysIncludeEventProps.RelatedMemberIds
        // };

        // return FilterDto(eventDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<EventDto>> ApplyPrivacyFilter(List<EventDto> eventDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<EventDto>();
        foreach (var eventDto in eventDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(eventDto, familyId, cancellationToken));
        }
        return filteredList;
    }

    public async Task<EventDetailDto> ApplyPrivacyFilter(EventDetailDto eventDetailDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return eventDetailDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicEventProperties(PrivacyConstants.DefaultPublicEventProperties.EventDetailDto);
        }

        var publicProperties = privacyConfig.GetPublicEventPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeEventProps.Id,
            PrivacyConstants.AlwaysIncludeEventProps.FamilyId,
            PrivacyConstants.AlwaysIncludeEventProps.FamilyName,
            PrivacyConstants.AlwaysIncludeEventProps.FamilyAvatarUrl,
            PrivacyConstants.AlwaysIncludeEventProps.RelatedMembers,
            PrivacyConstants.AlwaysIncludeEventProps.RelatedMemberIds,
            PrivacyConstants.AlwaysIncludeEventProps.Created,
            PrivacyConstants.AlwaysIncludeEventProps.CreatedBy,
            PrivacyConstants.AlwaysIncludeEventProps.LastModified,
            PrivacyConstants.AlwaysIncludeEventProps.LastModifiedBy
        };

        return FilterDto(eventDetailDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<FamilyDto> ApplyPrivacyFilter(FamilyDto familyDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        // if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        // {
        //     return familyDto;
        // }

        // PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        // if (privacyConfig == null)
        // {
        //     // If no config, create a default privacy config with predefined public properties
        //     privacyConfig = new PrivacyConfiguration(familyId);
        //     privacyConfig.UpdatePublicFamilyProperties(PrivacyConstants.DefaultPublicFamilyProperties.FamilyDto);
        // }

        // var publicProperties = privacyConfig.GetPublicFamilyPropertiesList();
        // var alwaysIncludeProps = new List<string>
        // {
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Id,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Name,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Code,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Visibility,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Created,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.CreatedBy,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.LastModified,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.LastModifiedBy,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.IsFollowing, // NEW
        // };

        // return FilterDto(familyDto, publicProperties, alwaysIncludeProps);
        return await Task.FromResult(familyDto);
    }

    public async Task<List<FamilyDto>> ApplyPrivacyFilter(List<FamilyDto> familyDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<FamilyDto>();
        foreach (var familyDto in familyDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(familyDto, familyId, cancellationToken));
        }
        return filteredList;
    }

    public async Task<FamilyDetailDto> ApplyPrivacyFilter(FamilyDetailDto familyDetailDto, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(familyDetailDto);
        // Admin always sees full data
        // if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        // {
        //     return familyDetailDto;
        // }

        // PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        // if (privacyConfig == null)
        // {
        //     // If no config, create a default privacy config with predefined public properties
        //     privacyConfig = new PrivacyConfiguration(familyId);
        //     privacyConfig.UpdatePublicFamilyProperties(PrivacyConstants.DefaultPublicFamilyProperties.FamilyDetailDto);
        // }

        // var publicProperties = privacyConfig.GetPublicFamilyPropertiesList();
        // var alwaysIncludeProps = new List<string>
        // {
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Id,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Name,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Code,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Visibility,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.Created,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.CreatedBy,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.LastModified,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.LastModifiedBy,
        //     PrivacyConstants.AlwaysIncludeFamilyProps.IsFollowing
        // };

        // return FilterDto(familyDetailDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<FamilyLocationDto> ApplyPrivacyFilter(FamilyLocationDto familyLocationDto, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(familyLocationDto);
        // Admin always sees full data
        // if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        // {
        //     return familyLocationDto;
        // }

        // PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        // if (privacyConfig == null)
        // {
        //     // If no config, create a default privacy config with predefined public properties
        //     privacyConfig = new PrivacyConfiguration(familyId);
        //     privacyConfig.UpdatePublicFamilyLocationProperties(PrivacyConstants.DefaultPublicFamilyLocationProperties.FamilyLocationDto);
        // }

        // var publicProperties = privacyConfig.GetPublicFamilyLocationPropertiesList();
        // var alwaysIncludeProps = new List<string>
        // {
        //     PrivacyConstants.AlwaysIncludeFamilyLocationProps.Id,
        //     PrivacyConstants.AlwaysIncludeFamilyLocationProps.FamilyId
        // };

        // return FilterDto(familyLocationDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<FamilyLocationDto>> ApplyPrivacyFilter(List<FamilyLocationDto> familyLocationDtos, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(familyLocationDtos);
        // var filteredList = new List<FamilyLocationDto>();
        // foreach (var familyLocationDto in familyLocationDtos)
        // {
        //     filteredList.Add(await ApplyPrivacyFilter(familyLocationDto, familyId, cancellationToken));
        // }
        // return filteredList;
    }

    public async Task<MemoryItemDto> ApplyPrivacyFilter(MemoryItemDto memoryItemDto, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(memoryItemDto);
        // Admin always sees full data
        // if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        // {
        //     return memoryItemDto;
        // }

        // PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        // if (privacyConfig == null)
        // {
        //     // If no config, create a default privacy config with predefined public properties
        //     privacyConfig = new PrivacyConfiguration(familyId);
        //     privacyConfig.UpdatePublicMemoryItemProperties(PrivacyConstants.DefaultPublicMemoryItemProperties.MemoryItemDto);
        // }

        // var publicProperties = privacyConfig.GetPublicMemoryItemPropertiesList();
        // var alwaysIncludeProps = new List<string>
        // {
        //     PrivacyConstants.AlwaysIncludeMemoryItemProps.Id,
        //     PrivacyConstants.AlwaysIncludeMemoryItemProps.FamilyId
        // };

        // return FilterDto(memoryItemDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<MemoryItemDto>> ApplyPrivacyFilter(List<MemoryItemDto> memoryItemDtos, Guid familyId, CancellationToken cancellationToken)
    {
        return await Task.FromResult(memoryItemDtos);
        // var filteredList = new List<MemoryItemDto>();
        // foreach (var memoryItemDto in memoryItemDtos)
        // {
        //     filteredList.Add(await ApplyPrivacyFilter(memoryItemDto, familyId, cancellationToken));
        // }
        // return filteredList;
    }

    public async Task<MemberFaceDto> ApplyPrivacyFilter(MemberFaceDto memberFaceDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberFaceDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberFaceProperties(PrivacyConstants.DefaultPublicMemberFaceProperties.MemberFaceDto);
        }

        var publicProperties = privacyConfig.GetPublicMemberFacePropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeMemberFaceProps.Id,
            PrivacyConstants.AlwaysIncludeMemberFaceProps.MemberId,
            PrivacyConstants.AlwaysIncludeMemberFaceProps.FamilyId,
            PrivacyConstants.AlwaysIncludeMemberFaceProps.BoundingBox
        };

        return FilterDto(memberFaceDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<MemberFaceDto>> ApplyPrivacyFilter(List<MemberFaceDto> memberFaceDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<MemberFaceDto>();
        foreach (var memberFaceDto in memberFaceDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(memberFaceDto, familyId, cancellationToken));
        }
        return filteredList;
    }

    public async Task<FoundFaceDto> ApplyPrivacyFilter(FoundFaceDto foundFaceDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUser.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return foundFaceDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            // FoundFaceDto should rely on MemberFace properties for privacy, or have its own
            // For now, let's assume it has its own default public properties.
            privacyConfig.UpdatePublicFoundFaceProperties(PrivacyConstants.DefaultPublicFoundFaceProperties.FoundFaceDto);
        }

        var publicProperties = privacyConfig.GetPublicFoundFacePropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            PrivacyConstants.AlwaysIncludeFoundFaceProps.MemberFaceId,
            PrivacyConstants.AlwaysIncludeFoundFaceProps.MemberId
        };

        return FilterDto(foundFaceDto, publicProperties, alwaysIncludeProps);
    }

    public async Task<List<FoundFaceDto>> ApplyPrivacyFilter(List<FoundFaceDto> foundFaceDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<FoundFaceDto>();
        foreach (var foundFaceDto in foundFaceDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(foundFaceDto, familyId, cancellationToken));
        }
        return filteredList;
    }
}
