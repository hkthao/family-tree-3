using backend.Application.Families.Queries;
using FluentValidation;

namespace backend.Application.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandValidator : AbstractValidator<CreateFamiliesCommand>
{
    public CreateFamiliesCommandValidator(IValidator<FamilyDto> familyDtoValidator)
    {
        RuleFor(x => x.Families)
            .NotEmpty().WithMessage("At least one family is required.");

        RuleForEach(x => x.Families).SetValidator(familyDtoValidator);
    }
}
