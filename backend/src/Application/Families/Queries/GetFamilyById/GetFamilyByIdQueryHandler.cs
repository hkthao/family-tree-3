using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var entity = await _context.Families.Find(Builders<Family>.Filter.Eq(f => f.Id, request.Id)).FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException(nameof(Family), request.Id);

        return _mapper.Map<FamilyDto>(entity);
    }
}