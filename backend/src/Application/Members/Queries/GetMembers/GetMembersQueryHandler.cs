using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.SearchMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MemberListDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var searchQuery = new SearchMembersQuery
        {
            SearchQuery = request.SearchTerm,
            FamilyId = request.FamilyId,
            Page = 1,
            ItemsPerPage = int.MaxValue
        };

        var spec = new MemberFilterSpecification(searchQuery, true);

        // Comment: Specification pattern is applied here to filter the results at the database level.
        var query = _context.Members.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var memberList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}