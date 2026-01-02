namespace backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;

/// <summary>
/// Validator cho GetVoiceProfileByIdQuery.
/// </summary>
public class GetVoiceProfileByIdQueryValidator : AbstractValidator<GetVoiceProfileByIdQuery>
{
    public GetVoiceProfileByIdQueryValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID hồ sơ giọng nói không được để trống.");
    }
}
