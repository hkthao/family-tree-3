using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
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
        var filter = Builders<Family>.Filter.Eq("_id", ObjectId.Parse(request.Id));
        var family = await _context.Families.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            throw new NotFoundException(nameof(Family), request.Id!);
        }

        return _mapper.Map<FamilyDto>(family);
    }
}
