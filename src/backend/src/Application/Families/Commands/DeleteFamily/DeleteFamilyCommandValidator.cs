namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandValidator : AbstractValidator<DeleteFamilyCommand>
{
    public DeleteFamilyCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");
    }
}