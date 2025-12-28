using backend.Application.Common.Extensions; // Add this using directive
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.SearchPublicMembers;

public class SearchPublicMembersQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<SearchPublicMembersQuery, Result<PaginatedList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<PaginatedList<MemberListDto>>> Handle(SearchPublicMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members
            .Include(m => m.Family) // NEW
            .AsNoTracking()
            .Where(m => m.FamilyId == request.FamilyId);

        // Apply search term
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(m => m.FirstName.Contains(request.SearchQuery) || m.LastName.Contains(request.SearchQuery) || m.Code.Contains(request.SearchQuery));
        }

        // Apply gender filter
        if (request.Gender.HasValue)
        {
            query = query.Where(m => m.Gender == request.Gender.Value.ToString());
        }

        // Apply isRoot filter
        if (request.IsRoot.HasValue)
        {
            query = query.Where(m => m.IsRoot == request.IsRoot.Value);
        }

        // Order by
        query = request.SortBy switch
        {
            "firstName" => request.SortOrder == "desc" ? query.OrderByDescending(m => m.FirstName) : query.OrderBy(m => m.FirstName),
            "lastName" => request.SortOrder == "desc" ? query.OrderByDescending(m => m.LastName) : query.OrderBy(m => m.LastName),
            "code" => request.SortOrder == "desc" ? query.OrderByDescending(m => m.Code) : query.OrderBy(m => m.Code),
            _ => query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName) // Default sort
        };

        var paginatedMemberEntities = await query
            .PaginatedListAsync(request.Page, request.ItemsPerPage); // Use request.Page and request.ItemsPerPage

        var memberListDtos = _mapper.Map<List<MemberListDto>>(paginatedMemberEntities.Items);

        var filteredMemberListDtos = new List<MemberListDto>();
        foreach (var memberListDto in memberListDtos)
        {
            filteredMemberListDtos.Add(await _privacyService.ApplyPrivacyFilter(memberListDto, request.FamilyId, cancellationToken));
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
