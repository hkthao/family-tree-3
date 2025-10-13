using FluentValidation;
using backend.Domain.Enums;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandValidator : AbstractValidator<SaveUserPreferencesCommand>
{
    public SaveUserPreferencesCommandValidator()
    {
        RuleFor(v => v.Theme)
            .IsInEnum().WithMessage("Invalid Theme value.");

        RuleFor(v => v.Language)
            .IsInEnum().WithMessage("Invalid Language value.");
    }
}