using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyDetailDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new FamilyByIdSpecification(request.Id);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = _context.Families.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyDto = await query
            .ProjectTo<FamilyDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyDto == null)
        {
            return Result<FamilyDetailDto>.Failure($"Family with ID {request.Id} not found.");
        }

        return Result<FamilyDetailDto>.Success(familyDto);
    }
}
