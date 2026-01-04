using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;

/// <summary>
/// Validator cho CreateVoiceProfileCommand.
/// </summary>
public class CreateVoiceProfileCommandValidator : AbstractValidator<CreateVoiceProfileCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateVoiceProfileCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID không được để trống.")
            .MustAsync(BeExistingMember).WithMessage("Member không tồn tại.")
            .MustAsync(NotExceedMaxActiveVoiceProfiles).WithMessage($"Mỗi thành viên chỉ được có tối đa {MAX_ACTIVE_VOICE_PROFILES} hồ sơ giọng nói đang hoạt động.");

        RuleFor(v => v.Label)
            .NotEmpty().WithMessage("Label không được để trống.")
            .MaximumLength(100).WithMessage("Label không được vượt quá 100 ký tự.");

        RuleFor(v => v.AudioUrl)
            .NotEmpty().WithMessage("Audio URL không được để trống.")
            .MaximumLength(500).WithMessage("Audio URL không được vượt quá 500 ký tự.")
            .Must(LinkMustBeAUri).WithMessage("Audio URL không hợp lệ.");

        RuleFor(v => v.DurationSeconds)
            .GreaterThan(0).WithMessage("Thời lượng audio phải lớn hơn 0.");

        RuleFor(v => v.Language)
            .NotEmpty().WithMessage("Ngôn ngữ không được để trống.")
            .MaximumLength(10).WithMessage("Ngôn ngữ không được vượt quá 10 ký tự.");
    }

    private bool LinkMustBeAUri(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return true;
        }
        return Uri.TryCreate(link, UriKind.Absolute, out _);
    }

    private async Task<bool> BeExistingMember(Guid memberId, CancellationToken cancellationToken)
    {
        return await _context.Members.AnyAsync(m => m.Id == memberId, cancellationToken);
    }

    private const int MAX_ACTIVE_VOICE_PROFILES = 2; // Rule 3: Mỗi Member nên có tối đa 1–2 VoiceProfile active

    private async Task<bool> NotExceedMaxActiveVoiceProfiles(Guid memberId, CancellationToken cancellationToken)
    {
        var activeVoiceProfilesCount = await _context.VoiceProfiles
            .CountAsync(vp => vp.MemberId == memberId && vp.Status == VoiceProfileStatus.Active, cancellationToken);

        return activeVoiceProfilesCount < MAX_ACTIVE_VOICE_PROFILES;
    }
}
