using backend.Application.Members.Queries; // Assuming AIMemberDto is in this namespace

namespace backend.Application.Members.Commands.CreateMembers;

public class AIMemberDtoValidator : AbstractValidator<AIMemberDto>
{
    public AIMemberDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Gender is required.")
            .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Gender must be a valid value (Male, Female, Other).");
    }
}
