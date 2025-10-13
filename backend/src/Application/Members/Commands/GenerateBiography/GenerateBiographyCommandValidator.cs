using FluentValidation;

namespace backend.Application.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandValidator : AbstractValidator<GenerateBiographyCommand>
{
    public GenerateBiographyCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId cannot be empty.");
    }
}
