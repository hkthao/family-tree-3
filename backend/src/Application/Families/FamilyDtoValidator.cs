namespace backend.Application.Families;

public class FamilyDtoValidator : AbstractValidator<FamilyDto>
{
    public FamilyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Visibility)
            .NotNull().WithMessage("Visibility is required.")
            .IsInEnum().WithMessage("Visibility must be a valid value (Public, Private, Shared).");
    }
}