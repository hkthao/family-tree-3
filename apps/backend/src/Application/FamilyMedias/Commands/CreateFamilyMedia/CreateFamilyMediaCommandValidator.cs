using FluentValidation;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMedia;

public class CreateFamilyMediaCommandValidator : AbstractValidator<CreateFamilyMediaCommand>
{
    public CreateFamilyMediaCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(v => v.File)
            .NotNull().WithMessage("File content is required.")
            .Must(file => file != null && file.Length > 0).WithMessage("File content cannot be empty.");

        RuleFor(v => v.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(250).WithMessage("File name must not exceed 250 characters.");

        RuleFor(v => v.MediaType)
            .IsInEnum().WithMessage("Invalid media type.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
