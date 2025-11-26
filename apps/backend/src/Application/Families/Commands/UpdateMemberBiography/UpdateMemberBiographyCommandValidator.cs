namespace backend.Application.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandValidator : AbstractValidator<UpdateMemberBiographyCommand>
{
    public UpdateMemberBiographyCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId cannot be empty.");

        RuleFor(v => v.BiographyContent)
            .MaximumLength(1500).WithMessage("Biography content must not exceed 1500 characters.");
    }
}
