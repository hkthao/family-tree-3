using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<SearchMembersQuery, Result<PaginatedList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<PaginatedList<MemberListDto>>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members
            .Include(m => m.Family) // NEW
            .Include(m => m.SourceRelationships)
                .ThenInclude(r => r.TargetMember)
            .Include(m => m.TargetRelationships)
                .ThenInclude(r => r.SourceMember)
            .AsQueryable();

        // Apply MemberAccessSpecification
        query = query.WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        // Apply individual specifications
        query = query.WithSpecification(new MemberSearchQuerySpecification(request.SearchQuery));
        query = query.WithSpecification(new MemberByGenderSpecification(request.Gender));
        query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new MemberByFatherIdSpecification(request.FatherId));
        query = query.WithSpecification(new MemberByMotherIdSpecification(request.MotherId));
        query = query.WithSpecification(new MemberByHusbandIdSpecification(request.HusbandId));
        query = query.WithSpecification(new MemberByWifeIdSpecification(request.WifeId));
        query = query.WithSpecification(new MemberOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<MemberListDto>>.Success(paginatedList);
    }
}
