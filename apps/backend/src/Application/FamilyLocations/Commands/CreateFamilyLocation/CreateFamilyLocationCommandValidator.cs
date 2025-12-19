namespace backend.Application.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandValidator : AbstractValidator<CreateFamilyLocationCommand>
{
    public CreateFamilyLocationCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId is required.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");
    }
}
