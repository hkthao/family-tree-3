using backend.Application.Common.Interfaces;

namespace backend.Application.VoiceProfiles.Commands.GenerateVoice;

/// <summary>
/// Validator cho GenerateVoiceCommand.
/// </summary>
public class GenerateVoiceCommandValidator : AbstractValidator<GenerateVoiceCommand>
{
    private readonly IApplicationDbContext _context;

    public GenerateVoiceCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.VoiceProfileId)
            .NotEmpty().WithMessage("Voice Profile ID không được để trống.")
            .MustAsync(BeExistingVoiceProfile).WithMessage("Voice Profile không tồn tại.");

        RuleFor(v => v.Text)
            .NotEmpty().WithMessage("Văn bản không được để trống.")
            .MaximumLength(4000).WithMessage("Văn bản không được vượt quá 4000 ký tự.");
    }

    private async Task<bool> BeExistingVoiceProfile(Guid voiceProfileId, CancellationToken cancellationToken)
    {
        return await _context.VoiceProfiles.AnyAsync(vp => vp.Id == voiceProfileId, cancellationToken);
    }
}
