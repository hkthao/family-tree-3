using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Application.Members.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, Result<MemberDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MemberDetailDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new MemberByIdSpecification(request.Id);
        spec.Query.Include(m => m.Relationships);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = _context.Members.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var memberDto = await query
            .ProjectTo<MemberDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDto == null)
        {
            return Result<MemberDetailDto>.Failure(new List<string> { $"Member with ID {request.Id} not found." });
        }

        return Result<MemberDetailDto>.Success(memberDto);
    }
}