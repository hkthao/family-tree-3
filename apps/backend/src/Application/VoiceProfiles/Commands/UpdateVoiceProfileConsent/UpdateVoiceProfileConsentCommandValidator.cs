namespace backend.Application.VoiceProfiles.Commands.UpdateVoiceProfileConsent;

/// <summary>
/// Validator cho UpdateVoiceProfileConsentCommand.
/// </summary>
public class UpdateVoiceProfileConsentCommandValidator : AbstractValidator<UpdateVoiceProfileConsentCommand>
{
    public UpdateVoiceProfileConsentCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID hồ sơ giọng nói không được để trống.");
    }
}
