using backend.Application.Common.Interfaces;

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler : IRequestHandler<GetFamiliesByIdsQuery, List<FamilyDto>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMapper _mapper;

    public GetFamiliesByIdsQueryHandler(IFamilyRepository familyRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _mapper = mapper;
    }

    public async Task<List<FamilyDto>> Handle(GetFamiliesByIdsQuery request, CancellationToken cancellationToken)
    {
        return (await _familyRepository.GetAllAsync())
            .Where(f => request.Ids.Contains(f.Id))
            .AsQueryable()
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .ToList();
    }
}
