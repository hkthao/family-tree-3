using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler : IRequestHandler<GetFamiliesByIdsQuery, List<FamilyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FamilyDto>> Handle(GetFamiliesByIdsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Families
            .Where(f => request.Ids.Contains(f.Id))
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
