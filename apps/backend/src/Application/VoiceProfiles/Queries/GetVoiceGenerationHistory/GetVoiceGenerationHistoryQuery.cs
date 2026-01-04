using backend.Application.Common.Interfaces;

using backend.Application.Common.Models; // Added

namespace backend.Application.VoiceProfiles.Queries.GetVoiceGenerationHistory;

/// <summary>
/// Query để lấy lịch sử tạo giọng nói của một hồ sơ giọng nói cụ thể.
/// </summary>
public record GetVoiceGenerationHistoryQuery : IRequest<Result<List<VoiceGenerationDto>>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói cần lấy lịch sử tạo.
    /// </summary>
    public Guid VoiceProfileId { get; init; }
}

/// <summary>
/// Validator cho GetVoiceGenerationHistoryQuery.
/// </summary>
public class GetVoiceGenerationHistoryQueryValidator : AbstractValidator<GetVoiceGenerationHistoryQuery>
{
    private readonly IApplicationDbContext _context;

    public GetVoiceGenerationHistoryQueryValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.VoiceProfileId)
            .NotEmpty().WithMessage("Voice Profile ID không được để trống.")
            .MustAsync(BeExistingVoiceProfile).WithMessage("Voice Profile không tồn tại.");
    }

    private async Task<bool> BeExistingVoiceProfile(Guid voiceProfileId, CancellationToken cancellationToken)
    {
        return await _context.VoiceProfiles.AnyAsync(vp => vp.Id == voiceProfileId, cancellationToken);
    }
}
