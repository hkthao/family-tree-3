using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.ExportVoiceProfiles;

/// <summary>
/// Xử lý truy vấn xuất danh sách hồ sơ giọng nói của một gia đình.
/// </summary>
public class ExportVoiceProfilesQueryHandler : IRequestHandler<ExportVoiceProfilesQuery, Result<List<VoiceProfileDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExportVoiceProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<VoiceProfileDto>>> Handle(ExportVoiceProfilesQuery request, CancellationToken cancellationToken)
    {
        // Lấy tất cả thành viên thuộc về FamilyId được cung cấp
        var memberIdsInFamily = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (!memberIdsInFamily.Any())
        {
            return Result<List<VoiceProfileDto>>.Success(new List<VoiceProfileDto>());
        }

        // Lấy tất cả hồ sơ giọng nói thuộc về các thành viên đã tìm thấy
        var voiceProfiles = await _context.VoiceProfiles
            .Where(vp => memberIdsInFamily.Contains(vp.MemberId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var voiceProfileDtos = _mapper.Map<List<VoiceProfileDto>>(voiceProfiles);

        return Result<List<VoiceProfileDto>>.Success(voiceProfileDtos);
    }
}
