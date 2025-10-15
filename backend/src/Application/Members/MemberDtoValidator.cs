using backend.Application.Members.Queries;

namespace backend.Application.Members;

public class MemberDtoValidator : AbstractValidator<MemberDto>
{
    public MemberDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Gender is required.")
            .IsInEnum().WithMessage("Gender must be a valid value (Male, Female, Other).");

        RuleFor(x => x.DateOfBirth)
            .NotNull().WithMessage("Date of birth is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}