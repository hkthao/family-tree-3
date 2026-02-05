using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models; // Add this using statement

namespace backend.Application.FamilyDicts.Queries.Public;

public class GetPublicFamilyDictByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPublicFamilyDictByIdQuery, Result<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<FamilyDictDto>> Handle(GetPublicFamilyDictByIdQuery request, CancellationToken cancellationToken)
    {
        var familyDict = await _context.FamilyDicts
            .Where(r => r.Id == request.Id)
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return familyDict == null
            ? Result<FamilyDictDto>.NotFound($"Public FamilyDict with ID {request.Id} not found.")
            : Result<FamilyDictDto>.Success(familyDict);
    }
}
