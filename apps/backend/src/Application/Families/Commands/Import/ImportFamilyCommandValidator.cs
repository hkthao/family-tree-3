using FluentValidation;
using backend.Domain.Enums; // Keep this, as Visibility enum is used

namespace backend.Application.Families.Commands.Import;

public class ImportFamilyCommandValidator : AbstractValidator<ImportFamilyCommand>
{
    public ImportFamilyCommandValidator()
    {
        RuleFor(v => v.FamilyData)
            .NotNull().WithMessage("FamilyData cannot be null.");

        RuleFor(v => v.FamilyData.Name)
            .NotEmpty().WithMessage("Family Name is required.")
            .MaximumLength(200).WithMessage("Family Name must not exceed 200 characters.");

        RuleFor(v => v.FamilyData.Code)
            .MaximumLength(50).WithMessage("Family Code must not exceed 50 characters.");

        RuleFor(v => v.FamilyData.Address)
            .MaximumLength(500).WithMessage("Family Address must not exceed 500 characters.");

        RuleFor(v => v.FamilyData.Visibility)
            .NotEmpty().WithMessage("Family Visibility is required.");
            // No IsEnumName validation, as Visibility is a string in domain entity

        RuleFor(v => v.FamilyData.FamilyHistory)
            .MaximumLength(2000).WithMessage("Family History must not exceed 2000 characters.");
    }
}