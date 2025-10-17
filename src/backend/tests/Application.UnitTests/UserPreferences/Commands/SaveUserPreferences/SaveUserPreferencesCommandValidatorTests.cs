using FluentValidation.TestHelper;
using Xunit;
using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandValidatorTests
{
    private readonly SaveUserPreferencesCommandValidator _validator;

    public SaveUserPreferencesCommandValidatorTests()
    {
        _validator = new SaveUserPreferencesCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Light,
            Language = Language.English
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenThemeIsInvalid()
    {
        var command = new SaveUserPreferencesCommand { Theme = (Theme)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Theme);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenLanguageIsInvalid()
    {
        var command = new SaveUserPreferencesCommand { Language = (Language)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }
}
