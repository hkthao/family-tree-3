namespace backend.Application.FamilyMedia.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandValidator : AbstractValidator<CreateFamilyMediaCommand>
{
    public CreateFamilyMediaCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(v => v.File)
            .NotNull().WithMessage("File is required.")
            .Must(file => file != null && file.Length > 0).WithMessage("File cannot be empty.");

        RuleFor(v => v.File.FileName)
            .NotEmpty().WithMessage("File name cannot be empty.")
            .MaximumLength(250).WithMessage("File name must not exceed 250 characters.");

        RuleFor(v => v.File.ContentType)
            .NotEmpty().WithMessage("File content type cannot be empty.");

        RuleFor(v => v.MediaType)
            .IsInEnum().WithMessage("Invalid media type.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
