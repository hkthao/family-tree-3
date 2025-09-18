using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using backend.Application.Families;
using backend.Application.Members;
using Microsoft.EntityFrameworkCore;

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
        var family = await _context.Families.FindAsync(new object[] { request.FamilyId }, cancellationToken);

        if (family == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.FamilyId);
        }

        var members = await _context.Members.Where(m => m.FamilyId == request.FamilyId).ToListAsync(cancellationToken);
        var relationships = await _context.Relationships.ToListAsync(cancellationToken);

        var familyTreeDto = new FamilyTreeDto
        {
            Family = _mapper.Map<FamilyDto>(family),
            Members = _mapper.Map<List<MemberDto>>(members),
            Relationships = _mapper.Map<List<RelationshipDto>>(relationships)
        };

        return familyTreeDto;
    }
}
