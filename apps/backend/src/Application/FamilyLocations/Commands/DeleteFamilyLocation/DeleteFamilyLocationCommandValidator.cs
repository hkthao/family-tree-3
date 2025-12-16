using FluentValidation;

namespace backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;

public class DeleteFamilyLocationCommandValidator : AbstractValidator<DeleteFamilyLocationCommand>
{
    public DeleteFamilyLocationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
