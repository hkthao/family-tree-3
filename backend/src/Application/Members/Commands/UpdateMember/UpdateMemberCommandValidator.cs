using FluentValidation;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(v => v.FullName)
            .MaximumLength(200)
            .NotEmpty();
        RuleFor(v => v.FamilyId)
            .NotEmpty();
    }
}
