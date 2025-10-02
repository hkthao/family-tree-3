using backend.Application.Common.Interfaces;
using backend.Application.Families;
using backend.Application.Members;
using backend.Domain.Entities;

namespace backend.Application.FamilyTree.Queries.GetFamilyTreeJson;

public class GetFamilyTreeJsonQueryHandler : IRequestHandler<GetFamilyTreeJsonQuery, FamilyTreeDto>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetFamilyTreeJsonQueryHandler(IFamilyRepository familyRepository, IMemberRepository memberRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<FamilyTreeDto> Handle(GetFamilyTreeJsonQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetByIdAsync(request.FamilyId);

        if (family == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.FamilyId);
        }

        var members = (await _memberRepository.GetAllAsync())
            .Where(m => m.FamilyId == request.FamilyId)
            .ToList();

        var familyTreeDto = new FamilyTreeDto
        {
            Family = _mapper.Map<FamilyDto>(family),
            Members = _mapper.Map<List<MemberDto>>(members)
        };

        return familyTreeDto;
    }
}
