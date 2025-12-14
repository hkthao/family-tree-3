using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Add this using statement

namespace backend.Application.FamilyDicts.Queries.Public;

public record GetPublicFamilyDictByIdQuery(Guid Id) : IRequest<Result<FamilyDictDto>>;

public class GetPublicFamilyDictByIdQueryHandler : IRequestHandler<GetPublicFamilyDictByIdQuery, Result<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPublicFamilyDictByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

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
