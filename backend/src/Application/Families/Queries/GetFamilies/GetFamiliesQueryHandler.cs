using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Families;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Driver;

namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandler : IRequestHandler<GetFamiliesQuery, List<FamilyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FamilyDto>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {
        var families = await _context.Families.Find(_ => true).ToListAsync(cancellationToken);
        return _mapper.Map<List<FamilyDto>>(families);
    }
}
