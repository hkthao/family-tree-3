namespace backend.Application.VoiceProfiles.Commands.ArchiveVoiceProfile;

/// <summary>
/// Validator cho ArchiveVoiceProfileCommand.
/// </summary>
public class ArchiveVoiceProfileCommandValidator : AbstractValidator<ArchiveVoiceProfileCommand>
{
    public ArchiveVoiceProfileCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID hồ sơ giọng nói không được để trống.");
    }
}
