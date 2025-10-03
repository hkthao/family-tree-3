using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Application.Members.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersWithPaginationQueryHandler : IRequestHandler<GetMembersWithPaginationQuery, PaginatedList<MemberListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MemberListDto>> Handle(GetMembersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var spec = new MemberFilterSpecification(
            request.SearchTerm,
            request.CreatedAfter,
            (request.PageNumber - 1) * request.PageSize,
            request.PageSize);

        // Comment: Specification pattern is applied here to filter, sort, and page the results at the database level.
        var query = SpecificationEvaluator<Member>.GetQuery(_context.Members.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        return await PaginatedList<MemberListDto>.CreateAsync(query.ProjectTo<MemberListDto>(_mapper.ConfigurationProvider).AsNoTracking(), request.PageNumber, request.PageSize);
    }
}