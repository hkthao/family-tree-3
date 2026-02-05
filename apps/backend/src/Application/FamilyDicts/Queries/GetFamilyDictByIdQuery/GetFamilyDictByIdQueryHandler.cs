using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models; // Add this using statement

namespace backend.Application.FamilyDicts.Queries;

public class GetFamilyDictByIdQueryHandler : IRequestHandler<GetFamilyDictByIdQuery, Result<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyDictByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyDictDto>> Handle(GetFamilyDictByIdQuery request, CancellationToken cancellationToken)
    {
        var familyDict = await _context.FamilyDicts
            .Where(r => r.Id == request.Id)
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return familyDict == null
            ? Result<FamilyDictDto>.NotFound($"FamilyDict with ID {request.Id} not found.")
            : Result<FamilyDictDto>.Success(familyDict);
    }
}
