namespace backend.Application.VoiceProfiles.Commands.UpdateVoiceProfile;

/// <summary>
/// Validator cho UpdateVoiceProfileCommand.
/// </summary>
public class UpdateVoiceProfileCommandValidator : AbstractValidator<UpdateVoiceProfileCommand>
{
    public UpdateVoiceProfileCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID hồ sơ giọng nói không được để trống.");

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
}
