using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences;

/// <summary>
/// Kiểm thử đơn vị cho SaveUserPreferencesCommandValidator.
/// Đảm bảo rằng các giá trị Theme và Language được xác thực đúng cách.
/// </summary>
public class SaveUserPreferencesCommandValidatorTests
{
    private readonly SaveUserPreferencesCommandValidator _validator;

    public SaveUserPreferencesCommandValidatorTests()
    {
        _validator = new SaveUserPreferencesCommandValidator();
    }

    /// <summary>
    /// Kiểm tra khi lệnh hợp lệ với Theme và Language đúng.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Validation_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Dark,
            Language = Language.Vietnamese
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Kiểm tra khi Theme không hợp lệ.
    /// </summary>
    [Fact]
    public void Should_Have_Validation_Error_When_Theme_Is_Invalid()
    {
        // Arrange
        var command = new SaveUserPreferencesCommand
        {
            Theme = (Theme)999, // Giá trị Theme không hợp lệ
            Language = Language.English
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Theme)
              .WithErrorMessage("Invalid Theme value.");
    }

    /// <summary>
    /// Kiểm tra khi Language không hợp lệ.
    /// </summary>
    [Fact]
    public void Should_Have_Validation_Error_When_Language_Is_Invalid()
    {
        // Arrange
        var command = new SaveUserPreferencesCommand
        {
            Theme = Theme.Light,
            Language = (Language)999 // Giá trị Language không hợp lệ
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language)
              .WithErrorMessage("Invalid Language value.");
    }
}
