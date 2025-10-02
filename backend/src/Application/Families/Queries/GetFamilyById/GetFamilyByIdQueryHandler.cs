using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, FamilyDto>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandler(IFamilyRepository familyRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _mapper = mapper;
    }

    public async Task<FamilyDto> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _familyRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
        }

        return _mapper.Map<FamilyDto>(entity);
    }
}
