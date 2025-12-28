using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs; // Added
using Newtonsoft.Json;

namespace backend.Application.MemoryItems.Queries.ExportMemoryItems;

public class ExportMemoryItemsQueryHandler : IRequestHandler<ExportMemoryItemsQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExportMemoryItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(ExportMemoryItemsQuery request, CancellationToken cancellationToken)
    {
        var memoryItems = await _context.MemoryItems
            .Where(mi => mi.FamilyId == request.FamilyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!memoryItems.Any())
        {
            return Result<string>.Failure("Không tìm thấy kỷ vật nào cho gia đình này.");
        }

        var memoryItemDtos = _mapper.Map<List<MemoryItemDto>>(memoryItems);
        var json = JsonConvert.SerializeObject(memoryItemDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}

