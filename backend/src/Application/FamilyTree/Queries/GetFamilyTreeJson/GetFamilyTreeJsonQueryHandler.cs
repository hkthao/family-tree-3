using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using backend.Application.Families;
using backend.Application.Members;

namespace backend.Application.FamilyTree.Queries.GetFamilyTreeJson;

public class GetFamilyTreeJsonQueryHandler : IRequestHandler<GetFamilyTreeJsonQuery, FamilyTreeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyTreeJsonQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FamilyTreeDto> Handle(GetFamilyTreeJsonQuery request, CancellationToken cancellationToken)
    {
        var familyObjectId = ObjectId.Parse(request.FamilyId);
        var family = await _context.Families.Find(Builders<Family>.Filter.Eq("_id", familyObjectId)).FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            throw new NotFoundException(nameof(Family), request.FamilyId);
        }

        var members = await _context.Members.Find(m => m.FamilyId == familyObjectId).ToListAsync(cancellationToken);
        var relationships = await _context.Relationships.Find(_ => true).ToListAsync(cancellationToken);

        var familyTreeDto = new FamilyTreeDto
        {
            Family = _mapper.Map<FamilyDto>(family),
            Members = _mapper.Map<List<MemberDto>>(members),
            Relationships = _mapper.Map<List<RelationshipDto>>(relationships)
        };

        return familyTreeDto;
    }
}
