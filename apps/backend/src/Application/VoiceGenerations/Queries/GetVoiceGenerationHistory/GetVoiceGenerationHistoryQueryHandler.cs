using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Add this for Result<T>

namespace backend.Application.VoiceGenerations.Queries.GetVoiceGenerationHistory;

/// <summary>
/// Handler để xử lý GetVoiceGenerationHistoryQuery.
/// </summary>
public class GetVoiceGenerationHistoryQueryHandler : IRequestHandler<GetVoiceGenerationHistoryQuery, Result<List<VoiceGenerationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVoiceGenerationHistoryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<VoiceGenerationDto>>> Handle(GetVoiceGenerationHistoryQuery request, CancellationToken cancellationToken)
    {
        var voiceGenerations = await _context.VoiceGenerations
            .Where(vg => vg.VoiceProfileId == request.VoiceProfileId)
            .OrderByDescending(vg => vg.Created)
            .ProjectTo<VoiceGenerationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<VoiceGenerationDto>>.Success(voiceGenerations);
    }
}
