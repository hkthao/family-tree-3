using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
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

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new MemberSearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
        // Note: GetMembersQuery does not have explicit sorting or pagination parameters beyond what PaginatedQuery provides.
        // If sorting is needed, a separate MemberOrderingSpecification would be applied here.
        // Pagination is handled by the PaginatedListAsync extension method.

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var memberList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}