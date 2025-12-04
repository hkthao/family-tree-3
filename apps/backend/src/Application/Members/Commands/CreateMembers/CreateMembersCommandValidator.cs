namespace backend.Application.Members.Commands.CreateMembers;

public class CreateMembersCommandValidator : AbstractValidator<CreateMembersCommand>
{
    public CreateMembersCommandValidator()
    {
        RuleFor(x => x.Members)
            .NotEmpty().WithMessage("At least one member is required.");

        RuleForEach(x => x.Members).ChildRules(member =>
        {
            member.RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            member.RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            member.RuleFor(x => x.Gender)
                .NotNull().WithMessage("Gender is required.")
                .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Gender must be a valid value (Male, Female, Other).");

            member.RuleFor(x => x.DateOfBirth)
                .NotNull().WithMessage("Date of birth is required.");
        });
    }
}
