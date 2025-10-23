namespace backend.Application.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandValidator : AbstractValidator<GenerateFamilyDataCommand>
{
    public GenerateFamilyDataCommandValidator()
    {
        RuleFor(v => v.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(1000).WithMessage("Prompt must not exceed 1000 characters.");
    }
}
