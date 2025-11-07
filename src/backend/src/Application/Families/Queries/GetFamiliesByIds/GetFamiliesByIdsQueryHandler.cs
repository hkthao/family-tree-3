using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFamiliesByIdsQuery, Result<List<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<FamilyDto>>> Handle(GetFamiliesByIdsQuery request, CancellationToken cancellationToken)
    {
        var familyList = await _context.Families
            .WithSpecification(new FamiliesByIdsSpecification(request.Ids))
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyDto>>.Success(familyList);
    }
}
