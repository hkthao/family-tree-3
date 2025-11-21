using backend.Application.Common.Interfaces;

namespace backend.Application.FamilyDicts.Queries.Public;

public record GetPublicFamilyDictByIdQuery(Guid Id) : IRequest<FamilyDictDto?>;

public class GetPublicFamilyDictByIdQueryHandler : IRequestHandler<GetPublicFamilyDictByIdQuery, FamilyDictDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPublicFamilyDictByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FamilyDictDto?> Handle(GetPublicFamilyDictByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.FamilyDicts
            .Where(r => r.Id == request.Id)
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
