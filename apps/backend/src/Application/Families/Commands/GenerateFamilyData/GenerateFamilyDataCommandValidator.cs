using backend.Application.Families.Commands.GenerateFamilyData;
using FluentValidation;

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Validator cho GenerateFamilyDataCommand.
/// </summary>
public class GenerateFamilyDataCommandValidator : AbstractValidator<GenerateFamilyDataCommand>
{
    public GenerateFamilyDataCommandValidator()
    {
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Nội dung không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung không được vượt quá 2000 ký tự.");

        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("SessionId không được để trống.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId không được để trống.");
    }
}
