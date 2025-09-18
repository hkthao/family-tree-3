namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(v => v.FullName)
            .MaximumLength(200)
            .NotEmpty();

    }
}
