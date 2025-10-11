namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
{
    public UpdateFamilyCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Name)
            .MaximumLength(200)
            .NotEmpty();
    }
}
