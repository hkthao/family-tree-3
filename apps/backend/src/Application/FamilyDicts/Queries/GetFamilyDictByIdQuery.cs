using backend.Application.Common.Interfaces;

namespace backend.Application.FamilyDicts.Queries;

public record GetFamilyDictByIdQuery(Guid Id) : IRequest<FamilyDictDto?>;

public class GetFamilyDictByIdQueryHandler : IRequestHandler<GetFamilyDictByIdQuery, FamilyDictDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyDictByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FamilyDictDto?> Handle(GetFamilyDictByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.FamilyDicts
            .Where(r => r.Id == request.Id)
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
