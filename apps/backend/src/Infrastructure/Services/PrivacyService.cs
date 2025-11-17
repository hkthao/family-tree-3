using System.Reflection;
using backend.Application.Common.Interfaces; // For ICurrentUserService
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById; // For MemberDetailDto
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Application.PrivacyConfigurations.Queries;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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

    public async Task<MemberDto> ApplyPrivacyFilter(MemberDto memberDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUserService.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberDto;
        }

        var privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, all properties are public by default
            return memberDto;
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var filteredMemberDto = new MemberDto();

        // Always include Id, FamilyId, Code, IsRoot, AvatarUrl as they are essential or visual identifiers
        filteredMemberDto.Id = memberDto.Id;
        filteredMemberDto.FamilyId = memberDto.FamilyId;
        filteredMemberDto.Code = memberDto.Code;
        filteredMemberDto.IsRoot = memberDto.IsRoot;
        filteredMemberDto.AvatarUrl = memberDto.AvatarUrl; // AvatarUrl is often public for display

        // Dynamically copy properties that are marked as public
        foreach (var propName in publicProperties)
        {
            var memberDtoProperty = typeof(MemberDto).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (memberDtoProperty != null)
            {
                var value = memberDtoProperty.GetValue(memberDto);
                memberDtoProperty.SetValue(filteredMemberDto, value);
            }
        }

        return filteredMemberDto;
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

        var privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, all properties are public by default
            return memberDetailDto;
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var filteredMemberDetailDto = new MemberDetailDto();

        // Always include Id, FamilyId, IsRoot, AvatarUrl, and relationships
        filteredMemberDetailDto.Id = memberDetailDto.Id;
        filteredMemberDetailDto.FamilyId = memberDetailDto.FamilyId;
        filteredMemberDetailDto.IsRoot = memberDetailDto.IsRoot;
        filteredMemberDetailDto.AvatarUrl = memberDetailDto.AvatarUrl;
        filteredMemberDetailDto.SourceRelationships = memberDetailDto.SourceRelationships;
        filteredMemberDetailDto.TargetRelationships = memberDetailDto.TargetRelationships;

        // Dynamically copy properties that are marked as public
        foreach (var propName in publicProperties)
        {
            var memberDetailDtoProperty = typeof(MemberDetailDto).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (memberDetailDtoProperty != null)
            {
                var value = memberDetailDtoProperty.GetValue(memberDetailDto);
                memberDetailDtoProperty.SetValue(filteredMemberDetailDto, value);
            }
        }

        return filteredMemberDetailDto;
    }

    public async Task<MemberListDto> ApplyPrivacyFilter(MemberListDto memberListDto, Guid familyId, CancellationToken cancellationToken)
    {
        // Admin always sees full data
        if (_currentUserService.UserId != Guid.Empty && _authorizationService.IsAdmin())
        {
            return memberListDto;
        }

        var privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == familyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no config, all properties are public by default
            return memberListDto;
        }

        var publicProperties = privacyConfig.GetPublicMemberPropertiesList();
        var filteredMemberListDto = new MemberListDto();

        // Always include Id, Code, IsRoot, AvatarUrl, FamilyId, FamilyName, and relationship full names/avatars/genders
        filteredMemberListDto.Id = memberListDto.Id;
        filteredMemberListDto.Code = memberListDto.Code;
        filteredMemberListDto.IsRoot = memberListDto.IsRoot;
        filteredMemberListDto.AvatarUrl = memberListDto.AvatarUrl;
        filteredMemberListDto.FamilyId = memberListDto.FamilyId;
        filteredMemberListDto.FamilyName = memberListDto.FamilyName;

        filteredMemberListDto.FatherFullName = memberListDto.FatherFullName;
        filteredMemberListDto.FatherAvatarUrl = memberListDto.FatherAvatarUrl;
        filteredMemberListDto.FatherGender = memberListDto.FatherGender;
        filteredMemberListDto.MotherFullName = memberListDto.MotherFullName;
        filteredMemberListDto.MotherAvatarUrl = memberListDto.MotherAvatarUrl;
        filteredMemberListDto.MotherGender = memberListDto.MotherGender;
        filteredMemberListDto.HusbandFullName = memberListDto.HusbandFullName;
        filteredMemberListDto.HusbandAvatarUrl = memberListDto.HusbandAvatarUrl;
        filteredMemberListDto.HusbandGender = memberListDto.HusbandGender;
        filteredMemberListDto.WifeFullName = memberListDto.WifeFullName;
        filteredMemberListDto.WifeAvatarUrl = memberListDto.WifeAvatarUrl;
        filteredMemberListDto.WifeGender = memberListDto.WifeGender;
        // BirthDeathYears is read-only, so we don't set it here. It will be derived from DateOfBirth/DateOfDeath if they are public.


        // Dynamically copy properties that are marked as public
        foreach (var propName in publicProperties)
        {
            var memberListDtoProperty = typeof(MemberListDto).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (memberListDtoProperty != null)
            {
                var value = memberListDtoProperty.GetValue(memberListDto);
                memberListDtoProperty.SetValue(filteredMemberListDto, value);
            }
        }

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
