using backend.Application.Common.Interfaces;

namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandler : IRequestHandler<GetFamiliesQuery, List<FamilyDto>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandler(IFamilyRepository familyRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _mapper = mapper;
    }

    public async Task<List<FamilyDto>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {
        var families = await _familyRepository.GetAllAsync();
        return _mapper.Map<List<FamilyDto>>(families);
    }
}
