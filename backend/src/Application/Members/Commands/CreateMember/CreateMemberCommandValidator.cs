namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty();

        RuleFor(v => v.FirstName)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.LastName)
            .MaximumLength(100)
            .NotEmpty();
    }
}
