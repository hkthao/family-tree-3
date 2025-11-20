using backend.Application.Common.Extensions; // Add this using directive
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Queries.SearchPublicMembers;

public class SearchPublicMembersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchPublicMembersQuery, Result<PaginatedList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<MemberListDto>>> Handle(SearchPublicMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members
            .AsNoTracking()
            .Where(m => m.FamilyId == request.FamilyId);

        // Apply search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m => m.FirstName.Contains(request.SearchTerm) || m.LastName.Contains(request.SearchTerm) || m.Code.Contains(request.SearchTerm));
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

        var paginatedList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage); // Use request.Page and request.ItemsPerPage

        return Result<PaginatedList<MemberListDto>>.Success(paginatedList);
    }
}
