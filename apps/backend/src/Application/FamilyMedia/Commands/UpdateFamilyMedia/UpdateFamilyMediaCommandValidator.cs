namespace backend.Application.FamilyMedia.Commands.UpdateFamilyMedia;

public class UpdateFamilyMediaCommandValidator : AbstractValidator<UpdateFamilyMediaCommand>
{
    public UpdateFamilyMediaCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("FamilyMedia ID is required.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(v => v.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(250).WithMessage("File name must not exceed 250 characters.");

        RuleFor(v => v.MediaType)
            .IsInEnum().WithMessage("Invalid media type.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
