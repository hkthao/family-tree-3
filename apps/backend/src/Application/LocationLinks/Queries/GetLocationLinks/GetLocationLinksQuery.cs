using backend.Domain.Enums;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Queries.GetLocationLinks;

public record GetLocationLinksQuery : IRequest<Result<List<LocationLinkDto>>>
{
    public Guid? FamilyLocationId { get; init; }
    public string? RefId { get; init; }
    public RefType? RefType { get; init; }
}

public class GetLocationLinksQueryHandler : IRequestHandler<GetLocationLinksQuery, Result<List<LocationLinkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLocationLinksQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<LocationLinkDto>>> Handle(GetLocationLinksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LocationLinks.AsNoTracking();

        if (request.FamilyLocationId.HasValue)
        {
            query = query.Where(l => l.FamilyLocationId == request.FamilyLocationId.Value);
        }

        if (!string.IsNullOrEmpty(request.RefId))
        {
            query = query.Where(l => l.RefId == request.RefId);
        }

        if (request.RefType.HasValue)
        {
            query = query.Where(l => l.RefType == request.RefType.Value);
        }

        var list = await query
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<LocationLinkDto>>.Success(list);
    }
}
