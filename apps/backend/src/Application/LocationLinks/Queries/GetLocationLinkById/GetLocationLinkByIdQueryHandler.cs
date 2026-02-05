using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.LocationLinks.Queries.GetLocationLinkById;

public class GetLocationLinkByIdQueryHandler : IRequestHandler<GetLocationLinkByIdQuery, Result<LocationLinkDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLocationLinkByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LocationLinkDto>> Handle(GetLocationLinkByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.LocationLinks
            .Where(l => l.Id == request.Id)
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            return Result<LocationLinkDto>.Failure($"LocationLink with ID {request.Id} not found.");
        }

        return Result<LocationLinkDto>.Success(dto);
    }
}
