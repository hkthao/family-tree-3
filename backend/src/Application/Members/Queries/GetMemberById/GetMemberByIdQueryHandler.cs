using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Application.Members.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDetailDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var spec = new MemberByIdSpecification(request.Id);
        spec.AddInclude(m => m.Relationships);

        var query = SpecificationEvaluator<Member>.GetQuery(_context.Members.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var memberDto = await query
            .ProjectTo<MemberDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDto == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        return memberDto;
    }
}