using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Application.Members.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, IReadOnlyList<MemberListDto>>
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
        var spec = new MemberFilterSpecification(
            request.SearchTerm,
            request.CreatedAfter,
            0, // No skip
            int.MaxValue); // No take (get all)

        // Comment: Specification pattern is applied here to filter the results at the database level.
        var query = SpecificationEvaluator<Member>.GetQuery(_context.Members.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        return await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}