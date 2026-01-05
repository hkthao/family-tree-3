namespace backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;

public class UpdateFamilyLocationCommandValidator : AbstractValidator<UpdateFamilyLocationCommand>
{
    public UpdateFamilyLocationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId is required.");

        RuleFor(v => v.LocationId)
            .NotEmpty().WithMessage("LocationId is required.");

        RuleFor(v => v.LocationName)
            .NotEmpty().WithMessage("Location Name is required.")
            .MaximumLength(200).WithMessage("Location Name must not exceed 200 characters.");

        RuleFor(v => v.LocationDescription)
            .MaximumLength(1000).WithMessage("Location Description must not exceed 1000 characters.");

        RuleFor(v => v.LocationAddress)
            .MaximumLength(500).WithMessage("Location Address must not exceed 500 characters.");
    }
}
