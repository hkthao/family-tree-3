using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relations.Queries;

public record GetRelationByIdQuery(string Id) : IRequest<RelationDto?>;

public class GetRelationByIdQueryHandler : IRequestHandler<GetRelationByIdQuery, RelationDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRelationByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RelationDto?> Handle(GetRelationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Relations
            .Where(r => r.Id == request.Id)
            .ProjectTo<RelationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
