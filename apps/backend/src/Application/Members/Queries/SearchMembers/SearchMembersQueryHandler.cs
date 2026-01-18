using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;
using backend.Domain.Entities; // Added for Member entity

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<SearchMembersQuery, Result<PaginatedList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<PaginatedList<MemberListDto>>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        // 1. Initial query for members without Includes
        var baseQuery = _context.Members.AsNoTracking().AsQueryable();

        // Apply MemberAccessSpecification
        baseQuery = baseQuery.WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        // Apply individual specifications
        baseQuery = baseQuery.WithSpecification(new MemberSearchQuerySpecification(request.SearchQuery));
        baseQuery = baseQuery.WithSpecification(new MemberByGenderSpecification(request.Gender));
        baseQuery = baseQuery.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
        baseQuery = baseQuery.WithSpecification(new MemberByFatherIdSpecification(request.FatherId));
        baseQuery = baseQuery.WithSpecification(new MemberByMotherIdSpecification(request.MotherId));
        baseQuery = baseQuery.WithSpecification(new MemberByHusbandIdSpecification(request.HusbandId));
        baseQuery = baseQuery.WithSpecification(new MemberByWifeIdSpecification(request.WifeId));
        baseQuery = baseQuery.WithSpecification(new MemberOrderingSpecification(request.SortBy, request.SortOrder));

        // Get paginated raw member entities
        var paginatedMemberEntities = await PaginatedList<Member>.CreateAsync(baseQuery, request.Page, request.ItemsPerPage);

        if (!paginatedMemberEntities.Items.Any())
        {
            return Result<PaginatedList<MemberListDto>>.Success(
                new PaginatedList<MemberListDto>(new List<MemberListDto>(), 0, request.Page, 0)
            );
        }

        // 2. Collect all unique IDs for lookups
        var familyIds = paginatedMemberEntities.Items.Select(m => m.FamilyId).Distinct().ToList();
        var relatedMemberIds = paginatedMemberEntities.Items
            .SelectMany(m => new[] { m.FatherId, m.MotherId, m.HusbandId, m.WifeId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        // 3. Perform separate lookups with AsNoTracking()
        var familyLookup = await _context.Families
            .AsNoTracking()
            .Where(f => familyIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => new { f.Name, f.AvatarUrl }, cancellationToken);

        var relatedMembersLookup = await _context.Members
            .AsNoTracking()
            .Where(m => relatedMemberIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => new { m.FirstName, m.LastName, m.AvatarUrl }, cancellationToken);
        
        // 4. Map to DTOs and manually populate related fields
        var memberListDtos = _mapper.Map<List<MemberListDto>>(paginatedMemberEntities.Items);

        foreach (var dto in memberListDtos)
        {
            if (familyLookup.TryGetValue(dto.FamilyId, out var familyData))
            {
                dto.FamilyName = familyData.Name;
                dto.FamilyAvatarUrl = familyData.AvatarUrl;
            }

            if (dto.FatherId.HasValue && relatedMembersLookup.TryGetValue(dto.FatherId.Value, out var fatherData))
            {
                dto.FatherFullName = $"{fatherData.LastName} {fatherData.FirstName}";
                dto.FatherAvatarUrl = fatherData.AvatarUrl;
            }
            if (dto.MotherId.HasValue && relatedMembersLookup.TryGetValue(dto.MotherId.Value, out var motherData))
            {
                dto.MotherFullName = $"{motherData.LastName} {motherData.FirstName}";
                dto.MotherAvatarUrl = motherData.AvatarUrl;
            }
            if (dto.HusbandId.HasValue && relatedMembersLookup.TryGetValue(dto.HusbandId.Value, out var husbandData))
            {
                dto.HusbandFullName = $"{husbandData.LastName} {husbandData.FirstName}";
                dto.HusbandAvatarUrl = husbandData.AvatarUrl;
            }
            if (dto.WifeId.HasValue && relatedMembersLookup.TryGetValue(dto.WifeId.Value, out var wifeData))
            {
                dto.WifeFullName = $"{wifeData.LastName} {wifeData.FirstName}";
                dto.WifeAvatarUrl = wifeData.AvatarUrl;
            }
        }

        // 5. Apply privacy filter
        var filteredMemberListDtos = new List<MemberListDto>();
        foreach (var memberListDto in memberListDtos)
        {
            filteredMemberListDtos.Add(await _privacyService.ApplyPrivacyFilter(memberListDto, memberListDto.FamilyId, cancellationToken));
        }

        var filteredPaginatedList = new PaginatedList<MemberListDto>(
            filteredMemberListDtos,
            paginatedMemberEntities.TotalItems,
            paginatedMemberEntities.Page,
            paginatedMemberEntities.TotalPages
        );

        return Result<PaginatedList<MemberListDto>>.Success(filteredPaginatedList);
    }
}
