using System.Reflection;
using AutoMapper;
using backend.Application.Common.Interfaces; // For ICurrentUserService
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById; // For MemberDetailDto
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Application.PrivacyConfigurations.Queries;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class PrivacyService : IPrivacyService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public PrivacyService(IApplicationDbContext context, ICurrentUser currentUserService, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
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

        // Luôn bao gồm các thuộc tính thiết yếu hoặc nhận dạng
        foreach (var propName in alwaysProps)
        {
            var sourceProperty = sourceType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var targetProperty = sourceType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sourceProperty != null && targetProperty != null && targetProperty.CanWrite)
            {
                var value = sourceProperty.GetValue(sourceDto);
                targetProperty.SetValue(filteredDto, value);
            }
        }

        // Sao chép động các thuộc tính được đánh dấu là công khai
        foreach (var propName in allowedProps)
        {
            // Tránh sao chép lại các thuộc tính đã được sao chép trong alwaysProps
            if (alwaysProps.Contains(propName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var sourceProperty = sourceType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var targetProperty = sourceType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sourceProperty != null && targetProperty != null && targetProperty.CanWrite)
            {
                var value = sourceProperty.GetValue(sourceDto);
                targetProperty.SetValue(filteredDto, value);
            }
        }

        return filteredDto;
    }

    public async Task<MemberDto> ApplyPrivacyFilter(MemberDto memberDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUserService.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(new List<string>
            {
                "LastName",
                "FirstName",
                "Nickname",
                "Gender",
                "DateOfBirth",
                "DateOfDeath",
                "PlaceOfBirth",
                "PlaceOfDeath",
                "Occupation",
                "Biography",
            });
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string> { "Id", "FamilyId", "Code", "IsRoot", "AvatarUrl" };

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
        // Admin always sees full data
        if (_currentUserService.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberDetailDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(new List<string>
            {
                "LastName",
                "FirstName",
                "Nickname",
                "Gender",
                "DateOfBirth",
                "DateOfDeath",
                "PlaceOfBirth",
                "PlaceOfDeath",
                "Occupation",
                "Biography",
                "FatherFullName",
                "MotherFullName",
                "HusbandFullName",
                "WifeFullName"
            });
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            "Id", "FamilyId", "IsRoot", "AvatarUrl",
            "SourceRelationships", "TargetRelationships"
        };

        return FilterDto(memberDetailDto, publicProperties, alwaysIncludeProps);
    }
    public async Task<MemberListDto> ApplyPrivacyFilter(MemberListDto memberListDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUserService.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberListDto;
        }

        PrivacyConfiguration? privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, create a default privacy config with predefined public properties
            privacyConfig = new PrivacyConfiguration(familyId);
            privacyConfig.UpdatePublicMemberProperties(new List<string>
            {
                "LastName",
                "FirstName",
                "Nickname",
                "Gender",
                "DateOfBirth",
                "DateOfDeath",
                "PlaceOfBirth",
                "PlaceOfDeath",
                "Occupation",
                "Biography",
                "FatherFullName",
                "MotherFullName",
                "HusbandFullName",
                "WifeFullName"
            });
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var alwaysIncludeProps = new List<string>
        {
            "Id", "Code", "IsRoot", "AvatarUrl", "FamilyId", "FamilyName",
            "FatherId", "MotherId", "HusbandId", "WifeId"
        };

        var filteredMemberListDto = FilterDto(memberListDto, publicProperties, alwaysIncludeProps);

        // Special handling for FullName if FirstName or LastName are private
        if (!publicProperties.Contains(nameof(MemberListDto.FirstName)) || !publicProperties.Contains(nameof(MemberListDto.LastName)))
        {
            filteredMemberListDto.FirstName = string.Empty;
            filteredMemberListDto.LastName = string.Empty;
            // FullName is derived, so it will be empty if FirstName/LastName are empty
        }

        return filteredMemberListDto;
    }

    public async Task<List<MemberListDto>> ApplyPrivacyFilter(List<MemberListDto> memberListDtos, Guid familyId, CancellationToken cancellationToken)
    {
        var filteredList = new List<MemberListDto>();
        foreach (var memberListDto in memberListDtos)
        {
            filteredList.Add(await ApplyPrivacyFilter(memberListDto, familyId, cancellationToken));
        }
        return filteredList;
    }
}
