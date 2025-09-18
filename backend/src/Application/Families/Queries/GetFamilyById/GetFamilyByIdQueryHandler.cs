using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, FamilyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FamilyDto> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Families.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
        }

        return _mapper.Map<FamilyDto>(entity);
    }
}