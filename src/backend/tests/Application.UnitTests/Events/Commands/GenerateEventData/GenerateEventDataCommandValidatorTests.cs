using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Events.Commands.GenerateEventData;

namespace backend.Application.UnitTests.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandValidatorTests
{
    private readonly GenerateEventDataCommandValidator _validator;

    public GenerateEventDataCommandValidatorTests()
    {
        _validator = new GenerateEventDataCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Prompt' rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenPromptIsEmpty()
    {
        // Arrange
        var command = new GenerateEventDataCommand("");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Prompt);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi trường 'Prompt' hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenPromptIsValid()
    {
        // Arrange
        var command = new GenerateEventDataCommand("Generate an event about a wedding.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
