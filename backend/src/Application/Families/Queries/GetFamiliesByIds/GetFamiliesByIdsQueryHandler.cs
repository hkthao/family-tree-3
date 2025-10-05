using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler : IRequestHandler<GetFamiliesByIdsQuery, Result<List<FamilyDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<FamilyDto>>> Handle(GetFamiliesByIdsQuery request, CancellationToken cancellationToken)
    {
        var familyList = await _context.Families
            .Where(f => request.Ids.Contains(f.Id))
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyDto>>.Success(familyList);
    }
}
