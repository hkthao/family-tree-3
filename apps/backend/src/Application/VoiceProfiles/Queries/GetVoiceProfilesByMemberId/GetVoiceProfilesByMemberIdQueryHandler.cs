using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;

/// <summary>
/// Handler để xử lý GetVoiceProfilesByMemberIdQuery.
/// </summary>
public class GetVoiceProfilesByMemberIdQueryHandler : IRequestHandler<GetVoiceProfilesByMemberIdQuery, Result<List<VoiceProfileDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVoiceProfilesByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<VoiceProfileDto>>> Handle(GetVoiceProfilesByMemberIdQuery request, CancellationToken cancellationToken)
    {
        // Check if member exists
        var memberExists = await _context.Members.AnyAsync(m => m.Id == request.MemberId, cancellationToken);
        if (!memberExists)
        {
            return Result<List<VoiceProfileDto>>.Failure(new Dictionary<string, string[]> { { "MemberId", new[] { "Member không tồn tại." } } });
        }

        var voiceProfiles = await _context.VoiceProfiles
            .Where(vp => vp.MemberId == request.MemberId)
            .ProjectTo<VoiceProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<VoiceProfileDto>>.Success(voiceProfiles);
    }
}
