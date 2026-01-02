namespace backend.Application.VoiceProfiles.Commands.ActivateVoiceProfile;

/// <summary>
/// Validator cho ActivateVoiceProfileCommand.
/// </summary>
public class ActivateVoiceProfileCommandValidator : AbstractValidator<ActivateVoiceProfileCommand>
{
    public ActivateVoiceProfileCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID hồ sơ giọng nói không được để trống.");
    }
}
