using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Application.Families.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, FamilyDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FamilyDetailDto> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new FamilyByIdSpecification(request.Id);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = SpecificationEvaluator<Family>.GetQuery(_context.Families.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyDto = await query
            .ProjectTo<FamilyDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyDto == null)
        {
            throw new NotFoundException(nameof(Family), request.Id);
        }

        return familyDto;
    }
}