using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs; // Added
using Newtonsoft.Json;

namespace backend.Application.MemoryItems.Queries.ExportMemoryItems;

public class ExportMemoryItemsQueryHandler : IRequestHandler<ExportMemoryItemsQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;

    public ExportMemoryItemsQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService)
    {
        _context = context;
        _mapper = mapper;
        _privacyService = privacyService;
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
        var filteredMemoryItemDtos = new List<MemoryItemDto>();
        foreach (var memoryItemDto in memoryItemDtos)
        {
            filteredMemoryItemDtos.Add(await _privacyService.ApplyPrivacyFilter(memoryItemDto, request.FamilyId, cancellationToken));
        }
        var json = JsonConvert.SerializeObject(filteredMemoryItemDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}

