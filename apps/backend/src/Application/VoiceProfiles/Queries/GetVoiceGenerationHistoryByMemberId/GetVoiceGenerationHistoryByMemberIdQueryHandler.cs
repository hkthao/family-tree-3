using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceGenerationHistoryByMemberId;

/// <summary>
/// Handler for GetVoiceGenerationHistoryByMemberIdQuery.
/// </summary>
public class GetVoiceGenerationHistoryByMemberIdQueryHandler : IRequestHandler<GetVoiceGenerationHistoryByMemberIdQuery, Result<List<VoiceGenerationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVoiceGenerationHistoryByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<VoiceGenerationDto>>> Handle(GetVoiceGenerationHistoryByMemberIdQuery request, CancellationToken cancellationToken)
    {
        var voiceGenerations = await _context.VoiceGenerations
            .Where(vg => vg.VoiceProfile.MemberId == request.MemberId) // Filter by MemberId through VoiceProfile
            .OrderByDescending(vg => vg.Created)
            .ProjectTo<VoiceGenerationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<VoiceGenerationDto>>.Success(voiceGenerations);
    }
}
