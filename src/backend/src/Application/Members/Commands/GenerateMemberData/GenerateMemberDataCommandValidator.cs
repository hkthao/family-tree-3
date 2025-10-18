namespace backend.Application.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandValidator : AbstractValidator<GenerateMemberDataCommand>
{
    public GenerateMemberDataCommandValidator()
    {
        RuleFor(v => v.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(1000).WithMessage("Prompt must not exceed 1000 characters.");
    }
}
