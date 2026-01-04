namespace backend.Application.VoiceProfiles.Commands.PreprocessAndCreateVoiceProfile;

public class PreprocessAndCreateVoiceProfileCommandValidator : AbstractValidator<PreprocessAndCreateVoiceProfileCommand>
{
    public PreprocessAndCreateVoiceProfileCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId is required.");

        RuleFor(v => v.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(100).WithMessage("Label must not exceed 100 characters.");

        RuleFor(v => v.RawAudioUrls)
            .NotEmpty().WithMessage("At least one RawAudioUrl is required.")
            .Must(list => list.All(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)))
            .WithMessage("All RawAudioUrls must be valid absolute URLs.");

        RuleFor(v => v.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Length(2).WithMessage("Language must be a 2-character code (e.g., 'en', 'vi').");

        RuleFor(v => v.Consent)
            .Equal(true).WithMessage("Consent is required to preprocess and create a voice profile.");
    }
}
