using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;

namespace backend.Application.MemoryItems.Queries.GetMemoryItemDetail;

public class GetMemoryItemDetailQueryHandler : IRequestHandler<GetMemoryItemDetailQuery, Result<MemoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemoryItemDetailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MemoryItemDto>> Handle(GetMemoryItemDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.MemoryItems
            .Where(mi => mi.Id == request.Id && mi.FamilyId == request.FamilyId && !mi.IsDeleted)
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
                .ThenInclude(mp => mp.Member) // Include Member details for MemoryPersonDto
            .ProjectTo<MemoryItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto == null ? Result<MemoryItemDto>.NotFound() : Result<MemoryItemDto>.Success(dto);
    }
}
